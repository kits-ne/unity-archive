namespace AndroidImmediateKit
{
    public abstract class SystemServiceBase : AndroidJavaObjectContext
    {
        protected SystemServiceBase(UnityPlayerActivity activity, string serviceName) :
            base(activity.GetSystemService(serviceName))
        {
        }
    }
}