using Nop.Data.Mapping;
using Nop.Plugin.Misc.Picker.Models;

namespace Nop.Plugin.Misc.Picker.Data
{
    public partial class OrderTrackMap : NopEntityTypeConfiguration<OrderTrack>
    {
        public OrderTrackMap()
        {
            this.ToTable("OrderTrack");
            this.HasKey(tr => tr.Id);
        }
    }
    public partial class PickerShippingInformationMap : NopEntityTypeConfiguration<PickerShippingInformation>
    {
        public PickerShippingInformationMap()
        {
            this.ToTable("PickerShippingInformation");
            this.HasKey(tr => tr.Id);
        }
    }

}
