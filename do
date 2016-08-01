[1mdiff --git a/PidgeyBot.csproj b/PidgeyBot.csproj[m
[1mindex 2fff72b..a9a589b 100644[m
[1m--- a/PidgeyBot.csproj[m
[1m+++ b/PidgeyBot.csproj[m
[36m@@ -22,7 +22,7 @@[m
     <DefineConstants>DEBUG;TRACE</DefineConstants>[m
     <ErrorReport>prompt</ErrorReport>[m
     <WarningLevel>4</WarningLevel>[m
[31m-    <Prefer32Bit>false</Prefer32Bit>[m
[32m+[m[32m    <Prefer32Bit>true</Prefer32Bit>[m
   </PropertyGroup>[m
   <PropertyGroup Condition=" '$(Configuration)|$(Platform)' == 'Release|AnyCPU' ">[m
     <PlatformTarget>AnyCPU</PlatformTarget>[m
[1mdiff --git a/Tasks/CatchLurePokemonsTask.cs b/Tasks/CatchLurePokemonsTask.cs[m
[1mindex c7c1ea2..3c2b378 100644[m
[1m--- a/Tasks/CatchLurePokemonsTask.cs[m
[1m+++ b/Tasks/CatchLurePokemonsTask.cs[m
[36m@@ -28,11 +28,11 @@[m [mnamespace PoGo.NecroBot.Logic.Tasks[m
             {[m
                 if (pidgey._client.Settings.AutoTransfer)[m
                 {[m
[31m-                    Logger.Write($"PokemonInventory is Full. Transferring pokemons...", LogLevel.Info);[m
[32m+[m[32m                    Logger.Write($"PokemonInventory is Full. Transferring pokemons...", LogLevel.Info, pidgey._trainerName, pidgey._authType);[m
                     await TransferDuplicatePokemonTask.Execute(pidgey);[m
                 }[m
                 else[m
[31m-                    Logger.Write($"PokemonInventory is Full. Please Transfer pokemon manually or set TransferDuplicatePokemon to true in settings...", LogLevel.Warning);[m
[32m+[m[32m                    Logger.Write($"PokemonInventory is Full. Please Transfer pokemon manually or set TransferDuplicatePokemon to true in settings...", LogLevel.Warning, pidgey._trainerName, pidgey._authType);[m
 [m
             }[m
             else[m
[1mdiff --git a/Tasks/FarmPokestopsTask.cs b/Tasks/FarmPokestopsTask.cs[m
[1mindex 29a912f..2d4b07b 100644[m
[1m--- a/Tasks/FarmPokestopsTask.cs[m
[1m+++ b/Tasks/FarmPokestopsTask.cs[m
[36m@@ -23,23 +23,6 @@[m [mnamespace PoGo.NecroBot.Logic.Tasks[m
     {[m
         public static async Task Execute(PidgeyInstance pidgey)[m
         {[m
[31m-            var distanceFromStart = LocationUtils.CalculateDistanceInMeters([m
[31m-                pidgey._client.Settings.DefaultLatitude, pidgey._client.Settings.DefaultLongitude,[m
[31m-                pidgey._client.CurrentLatitude, pidgey._client.CurrentLongitude);[m
[31m-[m
[31m-            // Edge case for when the client somehow ends up outside the defined radius[m
[31m-            if (pidgey._client.Settings.MaxTravelDistanceInMeters != 0 &&[m
[31m-                distanceFromStart > pidgey._client.Settings.MaxTravelDistanceInMeters)[m
[31m-            {[m
[31m-                await Task.Delay(5000);[m
[31m-[m
[31m-                Logger.Write(pidgey._client.CurrentLatitude + "," + pidgey._client.CurrentLongitude, LogLevel.Info, pidgey._trainerName, pidgey._authType);[m
[31m-[m
[31m-                await pidgey._navigation.HumanLikeWalking([m
[31m-                    new GeoCoordinate(pidgey._client.Settings.DefaultLatitude, pidgey._client.Settings.DefaultLongitude),[m
[31m-                    pidgey._client.Settings.WalkingSpeedInKilometerPerHour, null);[m
[31m-            }[m
[31m-[m
             var pokestopList = await GetPokeStops(pidgey);[m
             var stopsHit = 0;[m
 [m
[36m@@ -75,7 +58,6 @@[m [mnamespace PoGo.NecroBot.Logic.Tasks[m
                         await CatchIncensePokemonsTask.Execute(pidgey);[m
                         return true;[m
                     });[m
[31m-[m
                 //Catch Lure Pokemon[m
                 if (pokeStop.LureInfo != null)[m
                 {[m
[36m@@ -86,7 +68,7 @@[m [mnamespace PoGo.NecroBot.Logic.Tasks[m
                 string EggReward = fortSearch.PokemonDataEgg != null ? "1" : "0";[m
                 Logger.Write("Farmed XP: " + fortSearch.ExperienceAwarded + " Eggs: "+ EggReward + " Gems: "+fortSearch.GemsAwarded+" Items: " + GetSummedFriendlyNameOfItemAwardList(fortSearch.ItemsAwarded), Logger.LogLevel.Info, pidgey._trainerName, pidgey._authType);[m
 [m
[31m-                await Task.Delay(1000);[m
[32m+[m[32m                await Task.Delay(500);[m
                 if (++stopsHit % 5 == 0) //TODO: OR item/pokemon bag is full[m
                 {[m
                     stopsHit = 0;[m
[36m@@ -124,20 +106,14 @@[m [mnamespace PoGo.NecroBot.Logic.Tasks[m
         private static async Task<List<FortData>> GetPokeStops(PidgeyInstance pidgey)[m
         {[m
             var mapObjects = await pidgey._client.Map.GetMapObjects();[m
[31m-[m
[31m-            // Wasn't sure how to make this pretty. Edit as needed.[m
[31m-            var pokeStops = mapObjects.MapCells.SelectMany(i => i.Forts)[m
[32m+[m[32m            var pokeStops = mapObjects.MapCells[m
[32m+[m[32m                .SelectMany(i => i.Forts)[m
                 .Where([m
                     i =>[m
                         i.Type == FortType.Checkpoint &&[m
[31m-                        i.CooldownCompleteTimestampMs < DateTime.UtcNow.ToUnixTime() &&[m
[31m-                        ( // Make sure PokeStop is within max travel distance, unless it's set to 0.[m
[31m-                            LocationUtils.CalculateDistanceInMeters([m
[31m-                                pidgey._client.Settings.DefaultLatitude, pidgey._client.Settings.DefaultLongitude,[m
[31m-                                i.Latitude, i.Longitude) < pidgey._client.Settings.MaxTravelDistanceInMeters) ||[m
[31m-                        pidgey._client.Settings.MaxTravelDistanceInMeters == 0[m
[31m-                );[m
[31m-[m
[32m+[m[32m                        i.CooldownCompleteTimestampMs < DateTime.UtcNow.ToUnixTime())[m
[32m+[m[32m                .OrderBy(i => LocationUtils.CalculateDistanceInMeters(pidgey._client.CurrentLatitude,[m
[32m+[m[32m                        pidgey._client.CurrentLongitude, i.Latitude, i.Longitude));[m
             return pokeStops.ToList();[m
         }[m
     }[m
[1mdiff --git a/Tasks/UseNearbyPokestopsTask.cs b/Tasks/UseNearbyPokestopsTask.cs[m
[1mindex 0e48c5c..8b5db58 100644[m
[1m--- a/Tasks/UseNearbyPokestopsTask.cs[m
[1m+++ b/Tasks/UseNearbyPokestopsTask.cs[m
[36m@@ -70,20 +70,14 @@[m [mnamespace PoGo.NecroBot.Logic.Tasks[m
         private static async Task<List<FortData>> GetPokeStops(PidgeyInstance pidgey)[m
         {[m
             var mapObjects = await pidgey._client.Map.GetMapObjects();[m
[31m-[m
[31m-            // Wasn't sure how to make this pretty. Edit as needed.[m
[31m-            var pokeStops = mapObjects.MapCells.SelectMany(i => i.Forts)[m
[32m+[m[32m            var pokeStops = mapObjects.MapCells[m
[32m+[m[32m                .SelectMany(i => i.Forts)[m
                 .Where([m
                     i =>[m
                         i.Type == FortType.Checkpoint &&[m
[31m-                        i.CooldownCompleteTimestampMs < DateTime.UtcNow.ToUnixTime() &&[m
[31m-                        ( // Make sure PokeStop is within 40 meters or else it is pointless to hit it[m
[31m-                            LocationUtils.CalculateDistanceInMeters([m
[31m-                                pidgey._client.Settings.DefaultLatitude, pidgey._client.Settings.DefaultLongitude,[m
[31m-                                i.Latitude, i.Longitude) < 40) ||[m
[31m-                        pidgey._client.Settings.MaxTravelDistanceInMeters == 0[m
[31m-                );[m
[31m-[m
[32m+[m[32m                        i.CooldownCompleteTimestampMs < DateTime.UtcNow.ToUnixTime())[m
[32m+[m[32m                .OrderBy(i => LocationUtils.CalculateDistanceInMeters(pidgey._client.CurrentLatitude,[m
[32m+[m[32m                        pidgey._client.CurrentLongitude, i.Latitude, i.Longitude));[m
             return pokeStops.ToList();[m
         }[m
     }[m
warning: LF will be replaced by CRLF in Tasks/FarmPokestopsTask.cs.
The file will have its original line endings in your working directory.
warning: LF will be replaced by CRLF in Tasks/UseNearbyPokestopsTask.cs.
The file will have its original line endings in your working directory.
