using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Moq;
using ORM_Components;
using ORM_Components.Tables.Helpers;
using Moq.EntityFrameworkCore;
using System.Linq.Expressions;

namespace TestsBaseLib.Base;

public abstract class UnitTest
{
    protected readonly Mock<DataContext> _context;

    protected UnitTest()
    {
        _context = new Mock<DataContext>();
    }

    protected T any<T>() => It.IsAny<T>();

    protected List<T> itemsSetup<T>(
        Expression<Func<DataContext, DbSet<T>>> act,
        Expression<Func<DataContext, EntityEntry<T>>>? remove = null,
        Expression<Func<DataContext, EntityEntry<T>>>? add = null,
        Expression<Func<DataContext, ValueTask<T?>>>? find = null,
        Expression<Action<DataContext>>? removeRange = null
        )
        where T : IId
    {
        var items = new List<T>();

        _context.Setup(act).ReturnsDbSet(items);

        if (add != null)
            _context.Setup(add).Callback<T>(x => items.Add(x));

        if (remove != null)
            _context.Setup(remove).Callback<T>(x => items.Remove(x));

        if (find != null)
            _context.Setup(find).ReturnsFindAsync(items);

        if (removeRange != null)
            _context.Setup(removeRange).Callback<IEnumerable<T>>(x =>
            {
                foreach (var item in x)
                    items.Remove(item);
            });

        return items;
    }
}
