namespace Muonroi.BuildingBlock.External.Entity.DataSample
{
    public class InitialHostDbBuilder<TContext>(TContext context)
        where TContext : MDbContext
    {
        public void Create()
        {
            new DefaultLanguagesCreator<TContext>(context).Create();
            new HostRoleAndUserCreator<TContext>(context).Create();

            _ = context.SaveChanges();
        }
    }
}