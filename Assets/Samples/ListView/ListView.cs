using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using Inspectors.DataBinding;
using Shapes;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Samples.ListView
{
    public enum ListAxis
    {
        X = 0,
        Y = 1
    }

    [Serializable]
    public class ListItem
    {
        private RectTransform _rectTransform;
        [SerializeField] private Vector2 size;
        [SerializeField] private Vector2 position;

        public Vector2 Size
        {
            get => size;
            set
            {
                _rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, value.x);
                _rectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, value.y);
                size = value;
            }
        }

        public Vector2 Position
        {
            get => position;
            set
            {
                _rectTransform.anchoredPosition = value;
                position = value;
            }
        }

        public ListItem(RectTransform rectTransform)
        {
            _rectTransform = rectTransform;
            size = _rectTransform.rect.size;
            position = _rectTransform.anchoredPosition;
        }
    }

    [ExecuteAlways]
    [RequireComponent(typeof(RectTransform))]
    public class ListView : ImmediateModeShapeDrawer
    {
        [SerializeField] private RectTransform rectTransform;
        [SerializeField] private List<ItemViewUGUI> itemViewPrefabs;
        [SerializeField] private List<ListItem> listItems;
        [SerializeField] private Vector2 scrollPosition;
        [SerializeField] private ListAxis axis;
        [SerializeField] private LayoutGroup layout;
        private int AxisIndex => (int) axis;
        private Vector2 ViewSize => rectTransform.rect.size;

        [Header("Debug")] [SerializeField] private List<RectTransform> items;
        [SerializeField] private float rectThickness = 1;

        private readonly PrefabPool<int, ItemViewUGUI> _prefabPool = new();
        private ListWrapper DataWrapper { get; set; }
        private readonly Dictionary<int, (ItemViewUGUI Item, Type UserDataType)> _pagedInItems = new();

        private int _lowestPagedInIndex;
        private int _highestPagedInIndex;

        public void SetDataSource<T>(List<T> dataSource)
        {
            _prefabPool.Init(transform, itemViewPrefabs);

            Clear();

            DataWrapper?.Dispose();
            DataWrapper = ListWrapper<T>.Wrap(dataSource);

            if (dataSource != null)
            {
                InitializeView();
            }
        }

        private void InitializeView()
        {
            scrollPosition = Vector2.zero;
            _lowestPagedInIndex = 0;
            _highestPagedInIndex = -1;

            PopulateView(append: true);
        }

        private void Clear()
        {
            while (_pagedInItems.Count > 0)
            {
                PageOutItem(fromFront: false);
            }
        }

        private void PageOutItem(bool fromFront)
        {
            if (fromFront)
            {
                RemoveFromFront();
            }
            else
            {
                RemoveFromBack();
            }
        }

        private void RemoveFromFront()
        {
            RemoveListItem(_lowestPagedInIndex);
            _lowestPagedInIndex = Mathf.Min(DataWrapper.Count - 1, _lowestPagedInIndex + 1);
        }

        private void RemoveFromBack()
        {
            RemoveListItem(_highestPagedInIndex);
            _highestPagedInIndex = Mathf.Max(-1, _highestPagedInIndex - 1);
        }

        public int DataSourceItemCount => DataWrapper == null || DataWrapper.IsEmpty ? 0 : DataWrapper.Count;

        private void RemoveListItem(int index)
        {
            bool found = TryGetItemView(index, out var item);


            if (_pagedInItems.TryGetValue(index, out var value))
            {
                _pagedInItems.Remove(index);
            }

            if (found)
            {
                _prefabPool.ReturnPrefabInstance(item, index);
            }
        }

        private void PopulateView(bool append)
        {
            var usedSpace = GetUsedSpace();

            var viewSpace = ViewSize[AxisIndex];

            var emptySpace = viewSpace - usedSpace;
            Debug.Log($"Empty Space: {emptySpace.ToString(CultureInfo.InvariantCulture)}");
            if (emptySpace <= 0) return;

            while (emptySpace > 0)
            {
                var item = TryPageInItems(append, emptySpace);
                if (item == null) break;
                emptySpace -= item.Size[AxisIndex];
            }

            CalculateLayout();
        }

        private float GetUsedSpace()
        {
            if (listItems.Count == 0) return 0;

            var usedSpace = listItems[0].Position[AxisIndex];
            foreach (var listItem in listItems)
            {
                usedSpace += listItem.Size[AxisIndex];
            }

            return usedSpace;
        }

        private float GetEmptySpace() => ViewSize[AxisIndex] - GetUsedSpace();

        private void AddAxisPosition(ItemViewUGUI item, float delta)
        {
            var pos = item.Position;
            pos[AxisIndex] += delta;
            item.Position = pos;
        }

        private void SetAxisPosition(ItemViewUGUI item, float position)
        {
            var pos = item.Position;
            pos[AxisIndex] = position;
            item.Position = pos;
        }

        public void Scroll(float delta)
        {
            if (!enabled || DataWrapper == null || DataWrapper.IsEmpty)
            {
                return;
            }

            var axisIndex = AxisIndex;

            float clip = ViewSize[axisIndex] * 2;

            if (Mathf.Abs(delta) >= clip)
            {
                delta = Math.Sign(delta) * clip;
            }

            scrollPosition[AxisIndex] += delta;

            CalculateLayout();

            if (delta < 0)
            {
                var (lowItem, _) = _pagedInItems[_lowestPagedInIndex];
                var lowState = RectUtility.GetViewState(rectTransform.GetWorldRect(), lowItem.GetWorldRect());
                if (lowState == RectUtility.ViewState.InView)
                {
                    var prevItem = TryPageInItems(false, 0);
                    if (prevItem == null)
                    {
                        scrollPosition[AxisIndex] = 0;
                        CalculateLayout();
                    }
                }

                var (highItem, _) = _pagedInItems[_highestPagedInIndex];
                var highState = RectUtility.GetViewState(rectTransform.GetWorldRect(), highItem.GetWorldRect());
                if (highState == RectUtility.ViewState.OutOfView)
                {
                    PageOutItem(false);
                }
            }
            else
            {
                var (highItem, _) = _pagedInItems[_highestPagedInIndex];
                var highState = RectUtility.GetViewState(rectTransform.GetWorldRect(), highItem.GetWorldRect());
                if (highState == RectUtility.ViewState.InView)
                {
                    var nextItem = TryPageInItems(true, 0);
                    if (nextItem == null)
                    {
                        CalculateLayout();
                    }
                }

                var (lowItem, _) = _pagedInItems[_lowestPagedInIndex];
                var lowState = RectUtility.GetViewState(rectTransform.GetWorldRect(), lowItem.GetWorldRect());
                if (lowState == RectUtility.ViewState.OutOfView)
                {
                    PageOutItem(true);
                }
            }
        }

        private int SecondaryAxisItemCount => 1;

        private float JumpToIndexPageInternal(int index)
        {
            if (!enabled || DataWrapper == null || DataWrapper.IsEmpty)
            {
                return 0;
            }

            if (index < 0 || index >= DataWrapper.Count)
            {
                throw new IndexOutOfRangeException($"Expected: [0, {DataWrapper.Count}). Actual: {index}.");
            }

            bool inView = TryGetItemView(index, out _);
            bool pageInHigherOrderItems = index < _lowestPagedInIndex;

            if (!inView)
            {
                Clear();
                int paddedItems = index % SecondaryAxisItemCount;
                if (pageInHigherOrderItems)
                {
                    _lowestPagedInIndex = Mathf.Max(index - paddedItems + SecondaryAxisItemCount, 0);
                    _highestPagedInIndex = _lowestPagedInIndex - 1;
                }
                else
                {
                    _highestPagedInIndex = Mathf.Min(index - paddedItems + SecondaryAxisItemCount - 1,
                        DataWrapper.Count - 1);
                    _lowestPagedInIndex = _highestPagedInIndex + 1;
                }

                PopulateView(append: pageInHigherOrderItems);
                TryPageInItems(nextItem: false, 0);
            }

            float offset = 0;
            if (TryGetPrimaryAxisItemFromSourceIndex(index, out ItemViewUGUI listItem))
            {
                // float size = listItem.Size[AxisIndex];

                // if (!inView && pageInHigherOrderItems != layout.PositioningInverted)
                // {
                //     UIBlock.AutoLayout.Offset -= size + UIBlock.CalculatedSpacing.Value;
                // }

                // View.FinalizeItemsForView();
                // UIBlock.CalculateLayout();
                CalculateLayout();

                offset = ViewSize[AxisIndex];
            }

            return offset;
        }

        private bool TryGetPrimaryAxisItemFromSourceIndex(int index, out ItemViewUGUI item)
        {
            item = null;
            if (!TryGetItemView(index, out ItemViewUGUI listItem))
            {
                return false;
            }

            item = listItem;

            return true;
        }

        private bool TryGetItemView(int key, out ItemViewUGUI itemView)
        {
            if (!_pagedInItems.TryGetValue(key, out var value))
            {
                itemView = null;
                return false;
            }

            itemView = value.Item;
            return itemView != null;
        }

        public void JumpToIndex(int index)
        {
            Scroll(JumpToIndexPageInternal(index));
        }

        private void CalculateLayout()
        {
            var offset = scrollPosition;

            for (int i = _lowestPagedInIndex; i <= _highestPagedInIndex; i++)
            {
                var (item, _) = _pagedInItems[i];
                SetAxisPosition(item, offset[AxisIndex]);
                offset[AxisIndex] -= item.Size[AxisIndex];
            }
        }

        private ItemViewUGUI TryPageInItems(bool nextItem, float emptySpace)
        {
            if (DataWrapper == null)
            {
                return null;
            }

            bool addToFront = !nextItem && _lowestPagedInIndex > 0;
            bool addToBack = nextItem && _highestPagedInIndex < DataWrapper.Count - 1;

            if (!addToFront && !addToBack)
            {
                return null;
            }

            var listItem = PageInItem(firstSibling: addToFront);

            return listItem;
        }

        private ItemViewUGUI PageInItem(bool firstSibling)
        {
            return firstSibling ? PageInPreviousItem() : PageInNextItem();
        }

        private ItemViewUGUI PageInPreviousItem()
        {
            _lowestPagedInIndex--;

            if (!TryAddItem(_lowestPagedInIndex, out ItemViewUGUI item))
            {
                _lowestPagedInIndex++;
                return null;
            }

            item.SetAsFirstSibling();
            return item;
        }

        private ItemViewUGUI PageInNextItem()
        {
            _highestPagedInIndex++;
            if (!TryAddItem(_highestPagedInIndex, out var item))
            {
                _highestPagedInIndex--;
                return null;
            }

            item.SetAsLastSibling();

            return item;
        }

        private bool TryAddItem(int index, out ItemViewUGUI item)
        {
            if (index < 0 || index >= DataWrapper.Count)
            {
                // throw new IndexOutOfRangeException(
                // $"Index out of range. Expected [0, {DataWrapper.Count}) but received {index}.");
                item = null;
                return false;
            }

            var dataType = DataWrapper.GetDataType(index);
            var result = _prefabPool.GetPrefabInstance(index, dataType, out item, out var userDataType);

            if (item == null) return false;

            _pagedInItems[index] = (item, userDataType);

            if (_binders.TryGetValue((userDataType, item.TypeOfVisuals), out var binder) &&
                DataWrapper.TryGet(index, out object value))
            {
                binder.Bind(value, item.Visuals, index);
            }

            return true;
        }

        public void AddDataBinder<TData, TVisuals>(Action<TData, TVisuals, int> binder)
            where TVisuals : ItemVisuals
        {
            if (binder == null)
            {
                throw new ArgumentNullException(nameof(binder));
            }

            var key = (typeof(TData), typeof(TVisuals));
            if (_binders.ContainsKey(key)) return;
            var dataBinder = new DataBinder<TData, TVisuals>(binder);
            _binders.Add(key, dataBinder);
            _prefabPool.AddPrefabToDataTypeMapping<TVisuals, TData>();
        }

        private readonly Dictionary<(Type userDataType, Type prefabType), IDataBinder> _binders = new();

        public interface IDataBinder
        {
            void Bind(object data, object visuals, int index);
        }

        public class DataBinder<TData, TVisuals> : IDataBinder
        {
            private readonly Action<TData, TVisuals, int> _binder;

            public DataBinder(Action<TData, TVisuals, int> binder)
            {
                _binder = binder;
            }

            public void Bind(object data, object visuals, int index)
            {
                if (data is TData castData && visuals is TVisuals castVisuals)
                {
                    _binder.Invoke(castData, castVisuals, index);
                }
            }
        }

        public override void DrawShapes(Camera cam)
        {
            using (Draw.Command(cam))
            {
                var viewRect = rectTransform.GetWorldRect();
                Draw.Color = Color.gray;
                Draw.RectangleBorder(viewRect, rectThickness);

                foreach (var target in items)
                {
                    DrawItem(target);
                }

                foreach (var pagedInItem in _pagedInItems)
                {
                    DrawItem(pagedInItem.Value.Item.RectTransform);
                }

                void DrawItem(RectTransform target)
                {
                    var itemRect = target.GetWorldRect();
                    Draw.Color = RectUtility.GetViewState(viewRect, itemRect) switch
                    {
                        RectUtility.ViewState.InView => Color.green,
                        RectUtility.ViewState.OutOfView => Color.red,
                        _ => Color.gray
                    };
                    Draw.RectangleBorder(itemRect, rectThickness);
                }
            }
        }

        private void Reset()
        {
            rectTransform = GetComponent<RectTransform>();
        }
    }

    public enum PrefabRetrieval
    {
        Success,
        TypeMatchFailed,
        FailedToCreate
    }

    public class PrefabPool<TKey, TPrefab> where TPrefab : ItemView
    {
        private bool _initialized = false;
        private List<TPrefab> _itemPrefabs = new();
        private readonly Dictionary<TPrefab, Stack<TPrefab>> _prefabPools = new();
        private Dictionary<Type, List<Type>> prefabToDataTypeBinders = new();
        private Dictionary<TKey, TPrefab> pagedIn = new();

        private Transform _parent;

        public void Init(Transform parent, List<TPrefab> prefabs)
        {
            if (_initialized) return;
            _parent = parent;

            for (int i = 0; i < prefabs.Count; i++)
            {
                var itemPrefab = prefabs[i];
                if (itemPrefab == null) return;

                if (!itemPrefab.HasVisuals)
                {
                    Debug.LogError(
                        $"[{itemPrefab.name}] The {nameof(ItemView)}.{nameof(ItemView.Visuals)} property must be non-null in order to work correctly.",
                        itemPrefab);
                    continue;
                }

                if (_itemPrefabs.Contains(itemPrefab))
                {
                    continue;
                }

                _itemPrefabs.Add(itemPrefab);
                _prefabPools.Add(itemPrefab, new());
            }
        }

        private TPrefab GetSourcePrefab(Type dataType, out Type matchedDataType)
        {
            TPrefab prefab = null;
            matchedDataType = typeof(object);

            for (int i = 0; i < _itemPrefabs.Count; i++)
            {
                var listItemPrefab = _itemPrefabs[i];
                if (listItemPrefab == null) continue;
                if (!listItemPrefab.HasVisuals) continue;

                var prefabType = listItemPrefab.TypeOfVisuals;

                if (!prefabToDataTypeBinders.TryGetValue(prefabType, out List<Type> mappedDataTypes))
                {
                    continue;
                }

                for (int j = 0; j < mappedDataTypes.Count; ++j)
                {
                    Type mappedDataType = mappedDataTypes[j];

                    if (mappedDataType.IsAssignableFrom(dataType) &&
                        matchedDataType.IsAssignableFrom(mappedDataType))
                    {
                        matchedDataType = mappedDataType;
                        prefab = _itemPrefabs[i];
                    }
                }
            }

            return prefab;
        }

        public PrefabRetrieval GetPrefabInstance(
            TKey key, Type dataType,
            out TPrefab prefabInstance,
            out Type userDataType)
        {
            prefabInstance = null;
            TPrefab prefabSource = GetSourcePrefab(dataType, out userDataType);

            if (prefabSource == null)
            {
                return PrefabRetrieval.TypeMatchFailed;
            }

            if (!_prefabPools.TryGetValue(prefabSource, out Stack<TPrefab> prefabPool))
            {
                prefabPool = new Stack<TPrefab>();
                _prefabPools[prefabSource] = prefabPool;
            }

            // If prefabs in the pool were unexpectedly destroyed, try to handle that
            while (prefabInstance == null && prefabPool.Count > 0)
            {
                prefabInstance = prefabPool.Pop();
                prefabInstance.gameObject.SetActive(true);
            }

            if (prefabInstance == null)
            {
                // We cleared the pool but all the elements were invalid, try creating a new one
                try
                {
                    prefabInstance = Object.Instantiate(prefabSource, _parent);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Instantiating prefab {prefabSource.name} failed with: {e}", _parent);
                    return PrefabRetrieval.FailedToCreate;
                }
            }

            // else
            // {
            //     pagedOutItems.Remove(prefabInstance);
            //     pagedOutKeys.Remove(prefabInstance.UIBlock.ID);
            // }
            pagedIn.Add(key, prefabSource);

            if (!prefabInstance.gameObject.activeSelf)
            {
                try
                {
                    prefabInstance.gameObject.SetActive(true);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Activating prefab failed with: {e}", _parent);
                }
            }

            return PrefabRetrieval.Success;
        }

        public void ReturnPrefabInstance(TPrefab prefabInstance, TKey key)
        {
            // null check to handle the item being unexpectedly destroyed
            if (prefabInstance == null)
            {
                return;
            }

            if (!pagedIn.TryGetValue(key, out var prefab))
            {
                // if the prefab source was destroyed, we don't
                // know how to pool this... just destroy it
                try
                {
                    GameObject.Destroy(prefabInstance.gameObject);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }

                return;
            }

            pagedIn.Remove(key);

            Stack<TPrefab> prefabPool = _prefabPools[prefab];
            prefabPool.Push(prefabInstance);
            prefabInstance.gameObject.SetActive(false);
        }

        public void AddPrefabToDataTypeMapping<TPrefab, TData>()
        {
            Type prefabType = typeof(TPrefab);

            if (!prefabToDataTypeBinders.TryGetValue(prefabType, out List<Type> dataTypes))
            {
                dataTypes = new List<Type>();
                prefabToDataTypeBinders[prefabType] = dataTypes;
            }

            Type dataType = typeof(TData);
            if (!dataTypes.Contains(dataType))
            {
                dataTypes.Add(dataType);
            }
        }
    }

    public static class RectUtility
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool In(float value, float min, float max) => min <= value && value <= max;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool Out(float value, float min, float max) => value < min || max < value;

        public static ViewState GetViewState(Rect a, Rect b)
        {
            if (In(b.xMin, a.xMin, a.xMax) && In(b.xMax, a.xMin, a.xMax) &&
                In(b.yMin, a.yMin, a.yMax) && In(b.yMax, a.yMin, a.yMax))
            {
                return ViewState.InView;
            }

            if (Out(b.xMin, a.xMin, a.xMax) && Out(b.xMax, a.xMin, a.xMax) ||
                Out(b.yMin, a.yMin, a.yMax) && Out(b.yMax, a.yMin, a.yMax))
            {
                return ViewState.OutOfView;
            }

            return ViewState.Partial;
        }

        public enum ViewState
        {
            Partial,
            InView,
            OutOfView
        };
    }

    public static class RectTransformExtensions
    {
        public static Rect GetWorldRect(this RectTransform rectTransform)
        {
            var rect = rectTransform.rect;
            var matrix = rectTransform.localToWorldMatrix;
            rect.min = matrix.MultiplyPoint(rect.min);
            rect.max = matrix.MultiplyPoint(rect.max);
            return rect;
        }

        public static Rect GetWorldRect(this RectTransform rectTransform, Rect rect)
        {
            var matrix = rectTransform.localToWorldMatrix;
            rect.min = matrix.MultiplyPoint(rect.min);
            rect.max = matrix.MultiplyPoint(rect.max);
            return rect;
        }
    }
}