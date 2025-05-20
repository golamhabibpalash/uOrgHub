namespace OrgHub.Application.Features.FixedAssets.Constants;
public static class Permissions
{
    public static class Equipment
    {
        public const string Default = "FixedAssets.Equipment";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";
        public const string View = Default + ".View";
        public const string Export = Default + ".Export";
    }
}