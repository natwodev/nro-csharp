using Nro.Core.Util;
using Nro.Core.IO;
using Nro.Core.Logging;
using Nro.Core.Time;
using Nro.Game;
using Nro.Tests.Unit.Game.Support;
using Nro.Tests.Unit.Helpers;

namespace Nro.Tests.Unit.Game.Skills.Attributes;

public class AttributeTests
{
    private static Nro.Game.Skills.Attributes.Attribute Make(int value = 50, int time = 100) => new(1, new AttributeTemplate(4, "Tăng #value% kinh nghiệm"), value, time);

    [Fact]
    public void Update_counts_down_and_marks_changed_while_time_is_positive()
    {
        Nro.Game.Skills.Attributes.Attribute attribute = Make(time: 2);
        Assert.False(attribute.IsChanged);

        Assert.True(attribute.Update());
        Assert.Equal(1, attribute.Time);
        Assert.True(attribute.IsChanged);

        Assert.True(attribute.Update());
        Assert.Equal(0, attribute.Time);
        Assert.True(attribute.IsExpired());
        Assert.False(attribute.Update());
        Assert.Equal(0, attribute.Time);
    }

    [Fact]
    public void An_attribute_without_end_never_counts_down_and_never_expires()
    {
        Nro.Game.Skills.Attributes.Attribute attribute = Make(time: -1);

        Assert.False(attribute.Update());
        Assert.False(attribute.IsExpired());
        Assert.Equal(-1, attribute.Time);
        Assert.False(attribute.IsChanged);
    }

    [Fact]
    public void Setters_mark_the_attribute_changed()
    {
        Nro.Game.Skills.Attributes.Attribute attribute = Make();
        attribute.SetValue(70);
        Assert.Equal(70, attribute.Value);
        Assert.True(attribute.IsChanged);

        Nro.Game.Skills.Attributes.Attribute second = Make();
        second.SetTime(5);
        Assert.Equal(5, second.Time);
        Assert.True(second.IsChanged);

        Nro.Game.Skills.Attributes.Attribute third = Make();
        var other = new AttributeTemplate(9, "x");
        third.SetTemplate(other);
        Assert.Same(other, third.Template);
        Assert.True(third.IsChanged);
    }

    [Fact]
    public void ToString_is_the_template_text_with_the_value_and_the_time_left()
    {
        Assert.Equal("Tăng 50% kinh nghiệm trong vòng 1 giờ1 phút", Make(50, 3660).ToString());
        Assert.Equal("Tăng 50% kinh nghiệm", Make(50, -1).ToString());
        Assert.Equal("Tăng 50% kinh nghiệm trong vòng 1 phút", Make(50, 0).ToString());
    }

    [Fact]
    public void Construction_requires_a_template()
    {
        Assert.Throws<ArgumentNullException>(() => new Nro.Game.Skills.Attributes.Attribute(1, null!, 1, 1));
        Assert.Throws<ArgumentNullException>(() => Make().SetTemplate(null!));
    }
}

public class AttributeTemplateManagerTests
{
    [Fact]
    public async Task LoadAsync_reads_id_and_name_in_id_order()
    {
        var db = new FakeSqlExecutor().Table("from attribute_template", ["id", "name"],
            new object?[] { 1, "Kinh nghiệm" },
            new object?[] { 2, "Vàng" });
        var manager = new AttributeTemplateManager();

        await manager.LoadAsync(db);

        Assert.Equal(["Kinh nghiệm", "Vàng"], manager.Templates.Select(t => t.Name));
        Assert.Equal(2, manager.Find(2)!.Id);
        Assert.Null(manager.Find(3));
        Assert.Contains("order by id", db.QueryLog.Single());
    }

    [Fact]
    public async Task LoadAsync_failure_appends_nothing()
    {
        var db = new FakeSqlExecutor { FailQueryContaining = ("attribute_template", new InvalidOperationException("down")) };
        var manager = new AttributeTemplateManager();

        await Assert.ThrowsAsync<InvalidOperationException>(() => manager.LoadAsync(db));

        Assert.Empty(manager.Templates);
    }

    [Fact]
    public void Add_Find_and_Remove_work_on_the_same_objects()
    {
        var manager = new AttributeTemplateManager();
        var template = new AttributeTemplate(5, "a");
        manager.Add(template);

        Assert.Same(template, manager.Find(5));
        Assert.False(manager.Remove(new AttributeTemplate(5, "a")));
        Assert.True(manager.Remove(template));
        Assert.Null(manager.Find(5));
    }
}

public class AttributeManagerTests
{
    private static readonly DateTimeOffset Start = new(2026, 11, 3, 8, 0, 0, TimeSpan.Zero);

    private static (AttributeManager Manager, ManualTimeProvider Time, AttributeTemplateManager Templates) Make()
    {
        var time = new ManualTimeProvider(Start);
        var templates = new AttributeTemplateManager();
        templates.Add(new AttributeTemplate(1, "Kinh nghiệm #value"));
        templates.Add(new AttributeTemplate(2, "Vàng #value"));
        return (new AttributeManager(new VietnamClock(time)), time, templates);
    }

    private static Nro.Game.Skills.Attributes.Attribute Attr(AttributeTemplateManager templates, int id, int templateId, int value, int time) => new(id, templates.Find(templateId)!, value, time);

    [Fact]
    public void Find_returns_the_first_attribute_of_the_template()
    {
        (AttributeManager manager, _, AttributeTemplateManager templates) = Make();
        Nro.Game.Skills.Attributes.Attribute first = Attr(templates, 1, 2, 10, 5);
        manager.Add(first);
        manager.Add(Attr(templates, 2, 2, 20, 5));

        Assert.Same(first, manager.Find(2));
        Assert.Null(manager.Find(1));
    }

    [Fact]
    public void Update_runs_at_most_once_a_second_and_the_gap_is_exclusive()
    {
        (AttributeManager manager, ManualTimeProvider time, AttributeTemplateManager templates) = Make();
        Nro.Game.Skills.Attributes.Attribute attribute = Attr(templates, 1, 1, 10, 100);
        manager.Add(attribute);

        // lastUpdate starts at 0 and the clock is far past it: the first call runs.
        manager.Update();
        Assert.Equal(99, attribute.Time);

        time.Advance(TimeSpan.FromMilliseconds(1000));
        manager.Update();
        Assert.Equal(99, attribute.Time); // exactly 1000 ms: "greater than" is required

        time.Advance(TimeSpan.FromMilliseconds(1));
        manager.Update();
        Assert.Equal(98, attribute.Time);
    }

    [Fact]
    public void Update_skips_expired_attributes_and_keeps_endless_ones()
    {
        (AttributeManager manager, _, AttributeTemplateManager templates) = Make();
        Nro.Game.Skills.Attributes.Attribute expired = Attr(templates, 1, 1, 10, 0);
        Nro.Game.Skills.Attributes.Attribute endless = Attr(templates, 2, 2, 10, -1);
        Nro.Game.Skills.Attributes.Attribute running = Attr(templates, 3, 1, 10, 3);
        manager.Add(expired);
        manager.Add(endless);
        manager.Add(running);

        manager.Update();

        Assert.Equal(0, expired.Time);
        Assert.False(expired.IsChanged);
        Assert.Equal(-1, endless.Time);
        Assert.Equal(2, running.Time);
    }

    [Fact]
    public void SetTime_changes_the_attribute_but_always_answers_false_a_java_quirk()
    {
        (AttributeManager manager, _, AttributeTemplateManager templates) = Make();
        Nro.Game.Skills.Attributes.Attribute attribute = Attr(templates, 1, 1, 10, 3);
        manager.Add(attribute);

        bool found = manager.SetTime(1, 600);
        bool missing = manager.SetTime(99, 600);

        Assert.False(found);
        Assert.False(missing);
        Assert.Equal(600, attribute.Time);
        Assert.True(attribute.IsChanged);
    }

    [Fact]
    public void Remove_takes_out_that_object_only()
    {
        (AttributeManager manager, _, AttributeTemplateManager templates) = Make();
        Nro.Game.Skills.Attributes.Attribute attribute = Attr(templates, 1, 1, 10, 3);
        manager.Add(attribute);

        Assert.False(manager.Remove(Attr(templates, 1, 1, 10, 3)));
        Assert.True(manager.Remove(attribute));
        Assert.Empty(manager.Attributes);
    }

    [Fact]
    public void The_attribute_list_is_a_copy_so_walking_it_while_adding_is_safe()
    {
        (AttributeManager manager, _, AttributeTemplateManager templates) = Make();
        manager.Add(Attr(templates, 1, 1, 10, 3));

        foreach (Nro.Game.Skills.Attributes.Attribute _ in manager.Attributes)
        {
            manager.Add(Attr(templates, 2, 2, 10, 3));
        }

        Assert.Equal(2, manager.Attributes.Count);
    }

    [Fact]
    public async Task LoadAsync_reads_the_rows_and_links_the_templates()
    {
        (AttributeManager manager, _, AttributeTemplateManager templates) = Make();
        var db = new FakeSqlExecutor().Table("from attribute_server", ["id", "attribute_template_id", "value", "time"],
            new object?[] { 5, 2, 30, 3600 },
            new object?[] { 6, 1, 50, -1 });

        await manager.LoadAsync(db, templates);

        Assert.Equal(2, manager.Attributes.Count);
        Assert.Equal(5, manager.Attributes[0].Id);
        Assert.Equal("Vàng #value", manager.Attributes[0].Template.Name);
        Assert.Equal(30, manager.Attributes[0].Value);
        Assert.Equal(-1, manager.Attributes[1].Time);
        Assert.False(manager.Attributes[0].IsChanged);
        Assert.Contains("order by id", db.QueryLog.Single());
    }

    [Fact]
    public async Task LoadAsync_with_an_unknown_template_fails_and_appends_nothing()
    {
        (AttributeManager manager, _, AttributeTemplateManager templates) = Make();
        var db = new FakeSqlExecutor().Table("from attribute_server", ["id", "attribute_template_id", "value", "time"],
            new object?[] { 5, 2, 30, 3600 },
            new object?[] { 6, 77, 50, -1 });

        InvalidOperationException error = await Assert.ThrowsAsync<InvalidOperationException>(() => manager.LoadAsync(db, templates));

        Assert.Contains("77", error.Message);
        Assert.Empty(manager.Attributes);
    }

    [Fact]
    public async Task SaveChangedAsync_writes_only_changed_attributes_with_parameters_and_does_not_clear_the_flag()
    {
        (AttributeManager manager, _, AttributeTemplateManager templates) = Make();
        Nro.Game.Skills.Attributes.Attribute unchanged = Attr(templates, 5, 1, 10, 100);
        Nro.Game.Skills.Attributes.Attribute changed = Attr(templates, 6, 2, 20, 200);
        manager.Add(unchanged);
        manager.Add(changed);
        changed.SetValue(25);
        var db = new FakeSqlExecutor();

        int written = await manager.SaveChangedAsync(db);

        Assert.Equal(1, written);
        (string sql, object?[] args) = Assert.Single(db.Updates);
        Assert.Equal("update attribute_server set attribute_template_id = ?, value = ?, time = ? where id = ?", sql);
        Assert.Equal(new object?[] { 2, 25, 200, 6 }, args);
        Assert.True(changed.IsChanged);

        await manager.SaveChangedAsync(db);
        Assert.Equal(2, db.Updates.Count);
    }

    [Fact]
    public async Task SaveChangedAsync_with_nothing_changed_writes_nothing()
    {
        (AttributeManager manager, _, AttributeTemplateManager templates) = Make();
        manager.Add(Attr(templates, 5, 1, 10, 100));
        var db = new FakeSqlExecutor();

        Assert.Equal(0, await manager.SaveChangedAsync(db));
        Assert.Empty(db.Updates);
    }

    [Fact]
    public async Task Update_and_reads_from_many_threads_never_corrupt_the_list()
    {
        (AttributeManager manager, ManualTimeProvider time, AttributeTemplateManager templates) = Make();
        for (int i = 0; i < 20; i++)
        {
            manager.Add(Attr(templates, i, 1 + (i % 2), 1, 1_000_000));
        }

        var tasks = Enumerable.Range(0, 4).Select(t => Task.Run(() =>
        {
            for (int i = 0; i < 300; i++)
            {
                manager.Update();
                _ = manager.Find(1 + (i % 2));
                foreach (Nro.Game.Skills.Attributes.Attribute a in manager.Attributes)
                {
                    _ = a.ToString();
                }
            }
        }));

        await Task.WhenAll(tasks);

        Assert.Equal(20, manager.Attributes.Count);
        Assert.All(manager.Attributes, a => Assert.InRange(a.Time, 0, 1_000_000));
        _ = time;
    }
}
