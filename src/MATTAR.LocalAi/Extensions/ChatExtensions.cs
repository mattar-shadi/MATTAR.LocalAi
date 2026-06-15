using MATTAR.LocalAi.Abstractions;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.VectorData;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.SqliteVec;

namespace MATTAR.LocalAi.Extensions;

public static class ChatExtensions
{
    public static IServiceCollection AddChatForMaui(
        this IServiceCollection services)
    {
        services.AddTransient<IChat, Chat>();
        services.AddSingleton<IKnowledgeBase, KnowledgeBase>();
        services.AddChatSqliteMemory();

        return services;
    }

    public static IServiceCollection AddChatAgent(
        this IServiceCollection services)
    {
        services.AddTransient<IChat, ChatAgent>();
        services.AddSingleton<IKnowledgeBase, KnowledgeBase>();
        services.AddChatSqliteMemory();

        return services;
    }

    public static IServiceCollection AddFoundryLocalChatAgent(
        this IServiceCollection services)
    {
        services.AddTransient<IChat, FoundryLocalChatAgent>();
        services.AddSingleton<IKnowledgeBase, KnowledgeBase>();
        services.AddChatSqliteMemory();

        return services;
    }

    public static IServiceCollection AddChatSqliteMemory(
        this IServiceCollection services)
    {
//        services.AddSingleton<SqliteConnection>(sp =>
//        {
//#if DEBUG
//            var connection = new SqliteConnection("Data Source=:memory:");
//#else
//            var connection = new SqliteConnection("Data Source=database.db");
//#endif
//            connection.LoadExtension("vec0");

//            return connection;
//        });
//#if DEBUG
//        services.AddSqliteVectorStore("Data Source=:memory:", options);
//#else
//        services.AddSqliteVectorStore("Data Source=database.db", options);
//#endif

        //services.AddSingleton<IVectorStore, SqliteVectorStore>();
        services.AddSqliteVectorStore(_ => "Data Source=database.db");

        return services;
    }
}

