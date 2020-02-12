using System;

namespace TeamCloud.Azure.Resources
{
    public static class AzureRoleDefinition
    {
        public static Guid Owner = Guid.Parse("8e3af657-a8ff-443c-a75c-2fe8c4bcb635");

        public static Guid Contributor = Guid.Parse("b24988ac-6180-42a0-ab88-20f7382dd24c");

        public static Guid Reader = Guid.Parse("acdd72a7-3385-48ef-bd42-f606fba81ae7");
    }
}
