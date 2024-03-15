
namespace UniFramework.Machine
{
	/// <summary>
	/// 节点状态
	/// </summary>
	public interface IStateNode
	{
		void OnCreate(StateMachine machine);
		void OnEnter();
		void OnUpdate();
		void OnExit();
	}
}