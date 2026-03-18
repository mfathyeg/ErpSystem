using ErpSystem.Domain.Common.ValueObjects;
using ErpSystem.Modules.Configuration.Domain.Entities;
using ErpSystem.Modules.Configuration.Infrastructure.Persistence;
using ErpSystem.Modules.Finance.Domain.Entities;
using ErpSystem.Modules.Finance.Domain.ValueObjects;
using ErpSystem.Modules.Finance.Infrastructure.Persistence;
using ErpSystem.Modules.Inventory.Domain.Entities;
using ErpSystem.Modules.Inventory.Domain.ValueObjects;
using ErpSystem.Modules.Inventory.Infrastructure.Persistence;
using ErpSystem.Modules.Notifications.Domain.Entities;
using ErpSystem.Modules.Notifications.Domain.ValueObjects;
using ErpSystem.Modules.Notifications.Infrastructure.Persistence;
using ErpSystem.Modules.Orders.Domain.Entities;
using ErpSystem.Modules.Orders.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ErpSystem.API.Infrastructure;

public static class DataSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();

        await SeedInventoryAsync(scope.ServiceProvider);
        await SeedOrdersAsync(scope.ServiceProvider);
        await SeedFinanceAsync(scope.ServiceProvider);
        await SeedConfigurationAsync(scope.ServiceProvider);
        await SeedNotificationsAsync(scope.ServiceProvider);
    }

    private static async Task SeedInventoryAsync(IServiceProvider sp)
    {
        var context = sp.GetRequiredService<InventoryDbContext>();

        if (await context.Products.AnyAsync())
            return;

        var products = new[]
        {
            Product.Create("SKU-0001", "شاشة كمبيوتر 27 بوصة", "شاشة عالية الدقة للألعاب والعمل",
                ProductCategory.Create("Electronics", "إلكترونيات"),
                Money.Create(1500.00m, "SAR"), 50, 10),

            Product.Create("SKU-0002", "كرسي مكتب مريح", "كرسي مكتب مريح مع دعم للظهر",
                ProductCategory.Create("Furniture", "أثاث"),
                Money.Create(850.00m, "SAR"), 30, 5),

            Product.Create("SKU-0003", "لوحة مفاتيح لاسلكية", "لوحة مفاتيح لاسلكية مع إضاءة RGB",
                ProductCategory.Create("Accessories", "إكسسوارات"),
                Money.Create(250.00m, "SAR"), 100, 20),

            Product.Create("SKU-0004", "ماوس احترافي", "ماوس للألعاب عالي الدقة",
                ProductCategory.Create("Accessories", "إكسسوارات"),
                Money.Create(180.00m, "SAR"), 120, 25),

            Product.Create("SKU-0005", "مكتب خشبي", "مكتب خشبي عصري",
                ProductCategory.Create("Furniture", "أثاث"),
                Money.Create(2200.00m, "SAR"), 15, 3),

            Product.Create("SKU-0006", "طابعة ليزر", "طابعة ليزر ملونة متعددة الوظائف",
                ProductCategory.Create("Electronics", "إلكترونيات"),
                Money.Create(1200.00m, "SAR"), 25, 5),

            Product.Create("SKU-0007", "سماعات رأس", "سماعات رأس لاسلكية مع إلغاء الضوضاء",
                ProductCategory.Create("Accessories", "إكسسوارات"),
                Money.Create(450.00m, "SAR"), 60, 15),

            Product.Create("SKU-0008", "كاميرا ويب HD", "كاميرا ويب عالية الدقة للاجتماعات",
                ProductCategory.Create("Electronics", "إلكترونيات"),
                Money.Create(320.00m, "SAR"), 40, 10),

            Product.Create("SKU-0009", "حامل شاشة", "حامل شاشة قابل للتعديل",
                ProductCategory.Create("Accessories", "إكسسوارات"),
                Money.Create(280.00m, "SAR"), 35, 8),

            Product.Create("SKU-0010", "خزانة ملفات", "خزانة ملفات معدنية بثلاثة أدراج",
                ProductCategory.Create("Storage", "تخزين"),
                Money.Create(650.00m, "SAR"), 20, 5),

            Product.Create("SKU-0011", "لابتوب Dell", "لابتوب Dell للأعمال i7 16GB",
                ProductCategory.Create("Electronics", "إلكترونيات"),
                Money.Create(4500.00m, "SAR"), 10, 3),

            Product.Create("SKU-0012", "كرسي زوار", "كرسي زوار مريح",
                ProductCategory.Create("Furniture", "أثاث"),
                Money.Create(350.00m, "SAR"), 45, 10),

            Product.Create("SKU-0013", "قلم ذكي", "قلم ذكي للأجهزة اللوحية",
                ProductCategory.Create("Accessories", "إكسسوارات"),
                Money.Create(120.00m, "SAR"), 80, 20),

            Product.Create("SKU-0014", "شاحن لاسلكي", "شاحن لاسلكي سريع 15W",
                ProductCategory.Create("Accessories", "إكسسوارات"),
                Money.Create(95.00m, "SAR"), 150, 30),

            Product.Create("SKU-0015", "رف كتب", "رف كتب خشبي 5 رفوف",
                ProductCategory.Create("Storage", "تخزين"),
                Money.Create(420.00m, "SAR"), 25, 5)
        };

        context.Products.AddRange(products);
        await context.SaveChangesAsync();
    }

    private static async Task SeedOrdersAsync(IServiceProvider sp)
    {
        var context = sp.GetRequiredService<OrdersDbContext>();

        if (await context.Orders.AnyAsync())
            return;

        // Order 1 - Pending
        var order1 = Order.Create(
            Guid.NewGuid(),
            Address.Create("شارع الملك فهد", "الرياض", "الرياض", "المملكة العربية السعودية", "12345"),
            null,
            "SAR");
        order1.AddItem(Guid.NewGuid(), "شاشة كمبيوتر 27 بوصة", "SKU-0001", 2, Money.Create(1500.00m, "SAR"));
        order1.AddItem(Guid.NewGuid(), "لوحة مفاتيح لاسلكية", "SKU-0003", 1, Money.Create(250.00m, "SAR"));
        order1.Submit();
        context.Orders.Add(order1);
        await context.SaveChangesAsync();

        // Order 2 - Confirmed
        var order2 = Order.Create(
            Guid.NewGuid(),
            Address.Create("شارع العليا", "الرياض", "الرياض", "المملكة العربية السعودية", "12346"),
            null,
            "SAR");
        order2.AddItem(Guid.NewGuid(), "كرسي مكتب مريح", "SKU-0002", 3, Money.Create(850.00m, "SAR"));
        order2.Submit();
        order2.Confirm();
        context.Orders.Add(order2);
        await context.SaveChangesAsync();

        // Order 3 - Shipped
        var order3 = Order.Create(
            Guid.NewGuid(),
            Address.Create("شارع التحلية", "جدة", "مكة", "المملكة العربية السعودية", "21422"),
            null,
            "SAR");
        order3.AddItem(Guid.NewGuid(), "لابتوب Dell", "SKU-0011", 1, Money.Create(4500.00m, "SAR"));
        order3.AddItem(Guid.NewGuid(), "ماوس احترافي", "SKU-0004", 1, Money.Create(180.00m, "SAR"));
        order3.Submit();
        order3.Confirm();
        order3.Ship();
        context.Orders.Add(order3);
        await context.SaveChangesAsync();

        // Order 4 - Delivered
        var order4 = Order.Create(
            Guid.NewGuid(),
            Address.Create("شارع الأمير سلطان", "الدمام", "الشرقية", "المملكة العربية السعودية", "31411"),
            null,
            "SAR");
        order4.AddItem(Guid.NewGuid(), "سماعات رأس", "SKU-0007", 2, Money.Create(450.00m, "SAR"));
        order4.Submit();
        order4.Confirm();
        order4.Ship();
        order4.Deliver();
        context.Orders.Add(order4);
        await context.SaveChangesAsync();

        // Order 5 - Pending
        var order5 = Order.Create(
            Guid.NewGuid(),
            Address.Create("شارع الملك عبدالعزيز", "الرياض", "الرياض", "المملكة العربية السعودية", "12347"),
            null,
            "SAR");
        order5.AddItem(Guid.NewGuid(), "طابعة ليزر", "SKU-0006", 1, Money.Create(1200.00m, "SAR"));
        order5.Submit();
        context.Orders.Add(order5);
        await context.SaveChangesAsync();
    }

    private static async Task SeedFinanceAsync(IServiceProvider sp)
    {
        var context = sp.GetRequiredService<FinanceDbContext>();

        if (await context.Transactions.AnyAsync())
            return;

        var transactions = new List<Transaction>();
        var random = new Random(42);
        var categories = new[]
        {
            ("Sales", "Income"),
            ("Services", "Income"),
            ("Salaries", "Expense"),
            ("Utilities", "Expense"),
            ("Supplies", "Expense"),
            ("Rent", "Expense"),
            ("Marketing", "Expense"),
            ("Refunds", "Expense")
        };

        for (int i = 0; i < 30; i++)
        {
            var category = categories[random.Next(categories.Length)];
            var transactionType = TransactionType.Create(category.Item2);
            var amount = Money.Create(Math.Round((decimal)(random.NextDouble() * 10000 + 100), 2), "SAR");
            var date = DateTime.UtcNow.AddDays(-random.Next(1, 90));

            var transaction = Transaction.Create(
                transactionType,
                category.Item1,
                amount,
                $"معاملة {category.Item1} رقم {i + 1}",
                date);

            if (random.Next(10) > 3)
            {
                transaction.Complete();
            }

            transactions.Add(transaction);
        }

        context.Transactions.AddRange(transactions);
        await context.SaveChangesAsync();
    }

    private static async Task SeedConfigurationAsync(IServiceProvider sp)
    {
        var context = sp.GetRequiredService<ConfigurationDbContext>();

        if (!await context.CompanySettings.AnyAsync())
        {
            var companySettings = CompanySettings.Create(
                "شركة دورالوكس",
                "شارع الملك فهد، الرياض، المملكة العربية السعودية",
                "+966 11 234 5678",
                "info@duralux.sa",
                "SAR",
                "Asia/Riyadh",
                "DD/MM/YYYY");

            context.CompanySettings.Add(companySettings);
        }

        if (!await context.SystemConfigs.AnyAsync())
        {
            var configs = new[]
            {
                SystemConfig.Create("MAX_LOGIN_ATTEMPTS", "5", "Security", "الحد الأقصى لمحاولات تسجيل الدخول قبل القفل", true, "int"),
                SystemConfig.Create("SESSION_TIMEOUT", "30", "Security", "مهلة انتهاء الجلسة بالدقائق", true, "int"),
                SystemConfig.Create("LOW_STOCK_THRESHOLD", "10", "Inventory", "حد التنبيه الافتراضي للمخزون المنخفض", true, "int"),
                SystemConfig.Create("ORDER_PREFIX", "ORD", "Orders", "بادئة رقم الطلب", true, "string"),
                SystemConfig.Create("TAX_RATE", "15", "Finance", "نسبة ضريبة القيمة المضافة", true, "decimal"),
                SystemConfig.Create("CURRENCY", "SAR", "Finance", "العملة الافتراضية", true, "string"),
                SystemConfig.Create("DATE_FORMAT", "DD/MM/YYYY", "System", "تنسيق التاريخ", true, "string"),
                SystemConfig.Create("TIMEZONE", "Asia/Riyadh", "System", "المنطقة الزمنية", true, "string")
            };

            context.SystemConfigs.AddRange(configs);
        }

        await context.SaveChangesAsync();
    }

    private static async Task SeedNotificationsAsync(IServiceProvider sp)
    {
        var context = sp.GetRequiredService<NotificationsDbContext>();

        if (await context.Notifications.AnyAsync())
            return;

        var adminUserId = Guid.NewGuid();

        var notifications = new[]
        {
            Notification.Create(adminUserId, "مرحباً بك في النظام", "تم إعداد حسابك بنجاح. يمكنك البدء في استخدام النظام الآن.", NotificationType.System),
            Notification.Create(adminUserId, "طلب جديد", "تم استلام طلب جديد #ORD-2024-001", NotificationType.Order),
            Notification.Create(adminUserId, "تنبيه مخزون", "المخزون منخفض: شاشة 27 بوصة", NotificationType.Warning),
            Notification.Create(adminUserId, "دفعة مستلمة", "تم استلام دفعة بقيمة 1,250.00 ر.س", NotificationType.Success),
            Notification.Create(adminUserId, "تحديث النظام", "تم تحديث النظام بنجاح إلى الإصدار 2.0", NotificationType.System)
        };

        context.Notifications.AddRange(notifications);
        await context.SaveChangesAsync();
    }
}
