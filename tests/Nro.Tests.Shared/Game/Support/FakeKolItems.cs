using Nro.Core.Templates;
using Nro.Game;

namespace Nro.Tests.Shared.Game.Support;

/// <summary>Creates plain items without a catalog: template named "item&lt;id&gt;", the lock option is NOT added so tests can count exactly the options the code under test adds.</summary>
public class FakeKolItems : IKolTaskItemFactory
{
    private static readonly Dictionary<int, ItemOptionTemplate> OptionTemplates = new();

    public List<(int TemplateId, int Quantity)> Created { get; } = [];

    public Item CreateNewItemLock(int itemTemplateId, int quantity)
    {
        Created.Add((itemTemplateId, quantity));
        return new Item { Template = new ItemTemplate { Id = (short)itemTemplateId, Name = "item" + itemTemplateId }, Quantity = quantity };
    }

    public ItemOption CreateOption(int optionTemplateId, int param)
    {
        lock (OptionTemplates)
        {
            if (!OptionTemplates.TryGetValue(optionTemplateId, out ItemOptionTemplate? template))
            {
                template = new ItemOptionTemplate(optionTemplateId, "option" + optionTemplateId, 0);
                OptionTemplates[optionTemplateId] = template;
            }

            return new ItemOption(template, param);
        }
    }

    public Item CopyItem(Item item)
    {
        var copy = new Item { Template = item.Template, Quantity = item.Quantity, Info = item.Info, Content = item.Content, CreateTime = item.CreateTime };
        foreach (ItemOption io in item.ItemOptions)
        {
            copy.ItemOptions.Add(new ItemOption(io));
        }

        return copy;
    }
}
