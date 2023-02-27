
namespace Nop.Plugin.Misc.Picker
{
    public static class SendinBlueDefaults
    {
        /// <summary>
        /// Gets a name of the synchronization schedule task
        /// </summary>
        public static string SynchronizationTaskName => "Synchronization (Picker plugin)";

        /// <summary>
        /// Gets a type of the synchronization schedule task
        /// </summary>
        public static string SynchronizationTask => "Nop.Plugin.Misc.Picker.Services.SynchronizationTask";

        /// <summary>
        /// Gets a default synchronization period in hours
        /// </summary>
        public static int DefaultSynchronizationPeriod => 12;
    }
}
