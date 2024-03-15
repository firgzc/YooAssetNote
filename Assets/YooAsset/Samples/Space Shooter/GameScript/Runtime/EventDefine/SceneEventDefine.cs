using UniFramework.Event;

/// <summary>
/// 自定义事件
/// </summary>
public class SceneEventDefine
{
    /// <summary>
    /// 切换至主页事件
    /// </summary>
    public class ChangeToHomeScene : IEventMessage
    {
        public static void SendEventMessage()
        {
            var msg = new ChangeToHomeScene();
            UniEvent.SendMessage(msg);
        }
    }

    /// <summary>
    /// 切换至战斗事件
    /// </summary>
    public class ChangeToBattleScene : IEventMessage
    {
        public static void SendEventMessage()
        {
            var msg = new ChangeToBattleScene();
            UniEvent.SendMessage(msg);
        }
    }
}