public abstract class ConstructController
{
    protected ConstructGridTransformController transform;
    protected ConstructGenerator generator;
    protected ConstructModuleBuilder moduleBuilder;
    protected ConstructVoxelBlockVisualsController voxelVisuals;
    protected ConstructModelBlockVisualsController modelVisuals;

    private ModuleLocation? lastModuleLocation = null;

    protected ConstructController(
        ConstructGridTransformController transform,
        ConstructGenerator generator,
        ConstructModuleBuilder moduleBuilder,
        ConstructVoxelBlockVisualsController voxelVisuals,
        ConstructModelBlockVisualsController modelVisuals)
    {
        this.generator = generator;
        this.moduleBuilder = moduleBuilder;
        this.voxelVisuals = voxelVisuals;
        this.modelVisuals = modelVisuals;
    }

    public void Update(double delta, WorldGridPos loadPos)
    {
        ModuleLocation moduleLoadPos = loadPos.ToModuleLocation(transform);
        if (lastModuleLocation == null || lastModuleLocation != moduleLoadPos)
        {
            UpdateLoadingInternal(loadPos);
            lastModuleLocation = moduleLoadPos;
        }

        UpdateInternal(delta);
    }

    protected abstract void UpdateInternal(double delta);
    protected abstract void UpdateLoadingInternal(WorldGridPos loadPos);
}