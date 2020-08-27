using System;
namespace multibuzzer
{
    public static class StaticGuids
    {
        //services
        public static readonly Guid buzzerServiceGuid = Guid.Parse("3360cfb6-66fc-41cd-bf92-c4cb493eb399");

        //characteristics
        public static readonly Guid lockStatusGuid    = Guid.Parse("6eadd0d7-341c-428a-89d5-9c6d84aa12cf");

        public static readonly Guid buzzCharGuid      = Guid.Parse("8fe27cb3-ff97-43f0-bba1-8425441676ac");

        public static readonly Guid identityCharGuid  = Guid.Parse("d7f054f2-7ca7-4bc8-a6b2-675c9c8b3f79");

        public static readonly Guid teamCharGuid      = Guid.Parse("b0f79e9d-a7b3-459b-a9d9-ea25422cc900");
    }
}
