using MiNegocioCR.Api.Application.AI.Interfaces;
using MiNegocioCR.Api.Domain.Enums;

namespace MiNegocioCR.Api.Application.AI.Tools
{
    public class ToolSelector : IToolSelector
    {
        private readonly IEnumerable<IAITool> _tools;

        public ToolSelector(IEnumerable<IAITool> tools)
        {
            _tools = tools;
        }

        public IAITool Select(AIIntent intent)
        {
            return intent switch
            {
                AIIntent.RepairOrder =>
                    _tools.First(x => x.Name == "repair_order_search"),

                AIIntent.RepairService =>
                    _tools.First(x => x.Name == "repair_service_search"),

                AIIntent.Sales =>
                    _tools.First(x => x.Name == "sales_prepare"),

                _ =>
                    _tools.First(x => x.Name == "inventory_search")
            };
        }
    }
}
