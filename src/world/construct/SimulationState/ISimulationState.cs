public interface ISimulationState
{
    void OnEnter(Construct construct);
    void Update(double delta);
    void OnAddBlock(Block blockDefault, ConstructGridPos pos);
	void OnPositionChanged();
    void OnExit();
}