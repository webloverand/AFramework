namespace AFramework
{
    using UnityEngine;
    using System.Collections;
    using System.Collections.Generic;
    using System;
    public class IEnumeratorTool : MonoBehaviour
    {
        //协程索引值,依次增加
        static int counter = -1;
        //等待执行的协程的索引值列表
        static Queue<int> IEnumeratorQueue = new Queue<int>();
        //等待执行的协程
        static Dictionary<int, IEnumerator> iEnumeratorDictionary = new Dictionary<int, IEnumerator>();
        //正在执行的协程
        static Dictionary<int, Coroutine> coroutineDictionary = new Dictionary<int, Coroutine>();
        //需要停止的协程索引值
        static Queue<int> stopIEIdQueue = new Queue<int>();
        //是否需要停止所有协程
        static private bool isStopAllCroutine = false;

        /// <summary>
        /// 将协程加入等待列表
        /// </summary>
        /// <param name="ie"></param>
        /// <returns></returns>
        static public new int StartCoroutine(IEnumerator ie)
        {
            counter++;
            IEnumeratorQueue.Enqueue(counter);
            iEnumeratorDictionary[counter] = ie;
            return counter;
        }
        /// <summary>
        /// 根据ID停止协程
        /// </summary>
        /// <param name="id"></param>
        static public void StopCoroutine(int id)
        {
            stopIEIdQueue.Enqueue(id);
        }

        /// <summary>
        /// 停止全部协程
        /// </summary>
        static public void StopAllCroutine()
        {
            isStopAllCroutine = true;
        }

        /// <summary>
        /// 主循环
        /// </summary>
        void Update()
        {
            //停止所有协程
            if (isStopAllCroutine)
            {
                AFLogger.d("停止所有协程");
                StopAllCoroutines();
                isStopAllCroutine = false;
            }

            //优先停止协程
            while (stopIEIdQueue.Count > 0)
            {
                var id = stopIEIdQueue.Dequeue();
                Coroutine coroutine = null;
                if (coroutineDictionary.TryGetValue(id, out coroutine))
                {

                    base.StopCoroutine(coroutine);
                    coroutineDictionary.Remove(id);
                }
                else
                {
                    AFLogger.e(string.Format("此id协程不存在,无法停止:{0}", id));
                }
            }

            //协程循环
            while (IEnumeratorQueue.Count > 0)
            {
                var id = IEnumeratorQueue.Dequeue();
                //取出协程
                var ie = iEnumeratorDictionary[id];
                iEnumeratorDictionary.Remove(id);
                //执行协程
                var coroutine = base.StartCoroutine(ie);
                //存入coroutine
                coroutineDictionary[id] = coroutine;
            }
        }

        #region 协程实现延时执行函数
        /// <summary>
        /// 等待一段时间后执行action
        /// </summary>
        /// <param name="f"></param>
        /// <param name="action"></param>
        static public void WaitingForExec(float f, Action action)
        {
            StartCoroutine(IE_WaitingForExec(f, action));
        }

        static private IEnumerator IE_WaitingForExec(float f, Action action)
        {
            yield return new WaitForSecondsRealtime(f);
            if (action != null)
            {
                action();
            }
            yield break;
        }
        #endregion
    }
}
