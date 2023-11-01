//  Automatically generated
//

using BrunoMikoski.ScriptableObjectCollections;
using DefaultCompany.Samples.Collections;
using System.Collections.Generic;
using System;

namespace DefaultCompany.Samples.Collections
{
    public class ItemCollectionStatic
    {
        private static bool hasCachedValues;
        private static ItemCollection values;
        
        private static bool hasCachedItem0;
        private static DefaultCompany.Samples.Collections.Item cachedItem0;
        
        public static DefaultCompany.Samples.Collections.ItemCollection Values
        {
            get
            {
                if (!hasCachedValues)
                    hasCachedValues = CollectionsRegistry.Instance.TryGetCollectionByGUID(new LongGuid(5512404033760825611, 1470060596258753692), out values);
                return values;
            }
        }
        
        
        public static DefaultCompany.Samples.Collections.Item Item0
        {
            get
            {
                if (!hasCachedItem0)
                    hasCachedItem0 = Values.TryGetItemByGUID(new LongGuid(4975758575533691975, 8573315680354427041), out cachedItem0);
                return cachedItem0;
            }
        }
        
        
    }
}
