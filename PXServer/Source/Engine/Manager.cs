using NLog;
using PXServer.Source.Database;


namespace PXServer.Source.Engine;


public abstract class Manager
{
    protected readonly MongoDbContext Database;
    protected readonly Logger Logger;


    protected Manager(MongoDbContext database)
    {
        this.Database = database;
        this.Logger = LogManager.GetCurrentClassLogger();
    }
}