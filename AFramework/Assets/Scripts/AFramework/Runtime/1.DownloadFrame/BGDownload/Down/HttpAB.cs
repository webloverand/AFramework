namespace AFramework
{
    public delegate void ABProcessEvent(double process, bool isFinish,
        DownStatus downResult = DownStatus.Sucess, string downError = ""); //进度回调委托
    public class HttpAB : HttpBase
    {
        public HttpAB(HttpInfo aBDownInfo) : base(aBDownInfo)
        {

        }
    }
}
