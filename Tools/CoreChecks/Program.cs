using System;
using System.IO;
using TrainSummer;
class Program {
 static int checks;
 static void Check(bool condition,string name){if(!condition)throw new Exception(name);checks++;Console.WriteLine("PASS "+name);}
 static void Main(string[] args){
  if(args.Length==2 && args[0]=="--bake"){PatternExport.Export(args[1]);return;}
  string root=Path.Combine(Path.GetTempPath(),"TrainSummerChecks_"+Guid.NewGuid().ToString("N"));Directory.CreateDirectory(root);
  try{
   string path=Path.Combine(root,"save.json");
   AtomicFileStore.Write(path,"first — сохранение");Check(File.ReadAllText(path)=="first — сохранение","UTF-8 first write");
   AtomicFileStore.Write(path,"second");Check(File.ReadAllText(path)=="second","replacement committed");Check(File.ReadAllText(path+".bak")=="first — сохранение","previous save retained as backup");
   for(int i=0;i<20;i++)AtomicFileStore.Write(path,"value "+i);
   Check(File.ReadAllText(path)=="value 19" && File.ReadAllText(path+".bak")=="value 18","repeated atomic rotation");Check(!File.Exists(path+".tmp"),"temporary file cleaned");
   Check(AtomicFileStore.Hash("abc")=="ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad","SHA-256 known vector");Check(AtomicFileStore.Hash("first")!=AtomicFileStore.Hash("first!"),"changed payload has changed checksum");
   Check(!AtomicFileStore.Range(float.NaN,0,100) && !AtomicFileStore.Range(float.PositiveInfinity,0,100) && !AtomicFileStore.Range(-1,0,100),"non-finite and invalid ranges rejected");Check(AtomicFileStore.Range(0,0,100) && AtomicFileStore.Range(100,0,100),"range boundaries accepted");
   var bag=new InventoryModel();Check(!bag.TrySpend(1,0),"insufficient resources do not mutate state");
   Check(bag.TryAdd(4,2) && bag.TrySpend(3,1) && bag.MetalParts==1 && bag.Cloth==1,"repair resource transaction");
   Check(!bag.TryAdd(-1,0) && !bag.TrySpend(-1,0),"negative transactions rejected");
   Check(bag.TryTakePistol() && !bag.TryTakePistol() && bag.Magazine==12,"pistol ownership and initial magazine");
   bag.TryAddAmmo(24);for(int i=0;i<5;i++)bag.TrySpendRound();int total=bag.Magazine+bag.Ammo;
   Check(bag.Reload()==5 && bag.Magazine==12 && bag.Magazine+bag.Ammo==total,"reload conserves ammunition");
   for(int i=0;i<12;i++)bag.TrySpendRound();Check(!bag.TrySpendRound() && bag.Magazine==0,"empty magazine cannot fire");
   bag.TryTakeAxe();var snapshot=bag.Capture();var restored=new InventoryModel();restored.Restore(snapshot);
   Check(restored.Capture().weapon==snapshot.weapon && restored.Ammo==bag.Ammo && restored.HasAxe && restored.HasPistol,"inventory snapshot round trip");
   var invalid=bag.Capture();invalid.parts=-1;bool rejected=false;try{bag.Restore(invalid);}catch(ArgumentException){rejected=true;}
   Check(rejected && bag.MetalParts==1,"invalid snapshot rejected without partial mutation");
   bag.StackLimit=2;Check(!bag.TryAdd(3,0) && bag.MetalParts==1,"capacity failure is atomic");
   var invalidWeapon=new InventoryState{weapon=2,pistol=false};Check(!bag.IsValid(invalidWeapon),"unowned equipped weapon rejected");
   Check(bag.TryAddMedkit() && bag.TrySpendMedkit() && !bag.TrySpendMedkit(),"medkits consumed at most once");
   var craft=new InventoryModel();craft.TryAdd(10,10);
   Check(craft.TryCraft(CraftRecipe.RepairKit) && craft.MetalParts==7 && craft.Cloth==9 && craft.RepairKits==1,"repair kit recipe spends exact costs");
   Check(craft.TryCraft(CraftRecipe.Medkit) && craft.MetalParts==6 && craft.Cloth==7 && craft.Medkits==1,"medkit recipe spends exact costs");
   var insufficient=new InventoryModel();insufficient.TryAdd(5,0);Check(!insufficient.TryCraft(CraftRecipe.RepairKit) && insufficient.MetalParts==5,"craft failure leaves ingredients intact");
   var full=new InventoryModel{StackLimit=10};full.TryAdd(10,10);full.TryAddSupply(SurvivalItem.RepairKit,10);Check(!full.TryCraft(CraftRecipe.RepairKit) && full.MetalParts==10 && full.Cloth==10,"full output stack does not spend ingredients");
   Check(!craft.TryCraft((CraftRecipe)99) && craft.MetalParts==6,"unknown recipe rejected");
   Check(craft.TryAddSupply(SurvivalItem.Water,2) && craft.TryUseSupply(SurvivalItem.Water) && craft.Water==1,"water pickup and consumption");
   Check(!craft.TryAddSupply(SurvivalItem.Food,-1) && !craft.TryUseSupply(SurvivalItem.Food),"invalid or missing supplies cannot be consumed");
   var supplies=craft.Capture();var suppliesCopy=new InventoryModel();suppliesCopy.Restore(supplies);Check(suppliesCopy.Water==craft.Water && suppliesCopy.RepairKits==craft.RepairKits,"new supplies survive inventory round trip");
   var invalidSupplies=craft.Capture();invalidSupplies.food=-1;bool invalidRejected=false;try{craft.Restore(invalidSupplies);}catch(ArgumentException){invalidRejected=true;}Check(invalidRejected && craft.Food==0,"negative food restore rejected");
   var needs=new NeedsModel();needs.Restore(90,85);Check(needs.Drink() && needs.Thirst==100 && !needs.Drink(),"water saturates meter without overfilling");
   Check(needs.Eat() && needs.Hunger==100 && !needs.Eat(),"food saturates meter without overfilling");
   float foodBefore=needs.Hunger;needs.Advance(0);Check(needs.Hunger==foodBefore && needs.Thirst==100,"pause does not drain needs");
   needs.Restore(100,1);Check(Math.Abs(needs.Advance(10,0,.2f)-7.5f)<.0001f,"damage counts only time after a meter empties");
   needs.Restore(0,0);Check(needs.Advance(10,0,0)==15,"two empty meters do not double damage");
   bool invalidNeeds=false;try{needs.Restore(float.NaN,50);}catch(ArgumentException){invalidNeeds=true;}Check(invalidNeeds && needs.Hunger==0 && needs.Thirst==0,"invalid needs restore is atomic");
   var oneFrame=new NeedsModel();oneFrame.Restore(1,1);double totalDamage=0;var manyFrames=new NeedsModel();manyFrames.Restore(1,1);
   for(int i=0;i<1000;i++)totalDamage+=manyFrames.Advance(.1f,.025f,.045f);
   float oneDamage=oneFrame.Advance(100,.025f,.045f);Check(Math.Abs(totalDamage-oneDamage)<.001,"needs depletion is independent of frame subdivision");
   var window=new System.Collections.Generic.List<int>();EnvironmentLayout.Window(0,0,window);Check(window.Count==7 && window[0]==-2 && window[6]==4,"seven tiles around a boarding player");
   EnvironmentLayout.Window(0,-20,window);Check(window.Count==10 && window.Contains(-20) && window.Contains(-21) && window.Contains(-19),"stranded player keeps nearby ground");
   EnvironmentLayout.Window(10,13,window);Check(window.Count==7,"overlapping windows do not duplicate tiles");
   bool capacity=true;for(int i=-200;i<=200;i++){EnvironmentLayout.Window(i,-i,window);capacity&=window.Count>=7 && window.Count<=EnvironmentLayout.PoolCapacity;for(int j=1;j<window.Count;j++)capacity&=window[j]>window[j-1];}Check(capacity,"tile window stays unique and bounded across teleports");
   float[] stops={160,640,1200,1800};var treeA=EnvironmentLayout.Tree(4,13,stops);var treeB=EnvironmentLayout.Tree(4,13,stops);Check(treeA.distance==treeB.distance && treeA.offset==treeB.offset && treeA.scale==treeB.scale && treeA.species==treeB.species,"tree layout is deterministic");
   bool stationClear=true,ruinClear=true,trackClear=true,spacing=true,doors=true;
   for(int tile=-3;tile<60;tile++){
    var ruin=EnvironmentLayout.Ruin(tile,stops);if(ruin.active){foreach(float stop in stops)spacing&=Math.Abs(ruin.distance-stop)>=65;doors&=ruin.offset>0?ruin.yaw==90:ruin.yaw==-90;}
    for(int i=0;i<EnvironmentLayout.TreesPerTile;i++){
     var tree=EnvironmentLayout.Tree(tile,i,stops);trackClear&=Math.Abs(tree.offset)>=14;
     foreach(float stop in stops)if(tree.offset>0 && Math.Abs(tree.distance-stop)<32)stationClear&=tree.offset>=27;
     if(ruin.active && Math.Abs(tree.distance-ruin.distance)<8)ruinClear&=Math.Abs(tree.offset-ruin.offset)>=8;
    }
    for(int i=0;i<EnvironmentLayout.GrassPerTile;i++)trackClear&=Math.Abs(EnvironmentLayout.Grass(tile,i,stops).offset)>=2.8f;
   }
   Check(stationClear,"tree trunks avoid station footprint");Check(ruinClear,"tree trunks avoid pooled ruins");Check(trackClear,"vegetation leaves the track corridor clear");Check(spacing,"ruins avoid station stops");Check(doors,"ruin doors face the route");
   var shotgun=new InventoryModel();Check(!shotgun.TrySpendShell() && !shotgun.ReloadShell(),"unowned shotgun cannot fire or load");
   Check(shotgun.TryTakeShotgun() && !shotgun.TryTakeShotgun() && shotgun.Tube==5 && shotgun.SelectedWeapon==WeaponSlot.Shotgun,"shotgun picked up once with five shells");
   Check(shotgun.TryAddShells(12) && !shotgun.TryAddShells(-1),"shotgun reserve rejects negative amount");
   for(int i=0;i<5;i++)shotgun.TrySpendShell();Check(!shotgun.TrySpendShell() && shotgun.Tube==0,"empty tube cannot create a shot");
   int shellsBefore=shotgun.Tube+shotgun.Shells;Check(shotgun.ReloadShell() && shotgun.Tube==1 && shotgun.Shells==11 && shotgun.Tube+shotgun.Shells==shellsBefore,"single-shell loading conserves ammunition");
   for(int i=0;i<4;i++)shotgun.ReloadShell();Check(!shotgun.ReloadShell() && shotgun.Tube==5,"shotgun tube cannot overfill");
   var gunsCopy=new InventoryModel();gunsCopy.Restore(shotgun.Capture());Check(gunsCopy.HasShotgun && gunsCopy.Tube==5 && gunsCopy.Shells==7 && gunsCopy.SelectedWeapon==WeaponSlot.Shotgun,"shotgun snapshot round trip");
   var noShotgun=new InventoryState{tube=1};Check(!gunsCopy.IsValid(noShotgun) && !gunsCopy.IsValid(new InventoryState{weapon=3}),"unowned shotgun contents and equip rejected");
   var trip=new ExpeditionModel();Check(!trip.CanLeave(0) && !trip.Collect(-1) && !trip.Collect(4),"station objectives reject invalid relays and gate departure");
   Check(trip.Collect(0) && !trip.Collect(0) && trip.Recovered==1,"radio relay recovery is idempotent");
   trip.Advance(0,true,0,180);Check(trip.Remaining==180 && trip.TimedStop==0,"first arrival arms full dwell period");
   trip.Advance(100,false,0,180);Check(trip.Remaining==180,"travel does not consume station timer");
   trip.Advance(0,true,0,180);Check(trip.Remaining==180,"pause does not consume station timer");
   trip.Advance(181,true,0,180);Check(trip.Remaining==0 && trip.DepartureDue(true,0,true,true,true),"expired timer permits safe departure");
   Check(!trip.DepartureDue(true,0,false,true,true) && !trip.DepartureDue(true,0,true,false,true) && !trip.DepartureDue(true,0,true,true,false),"automatic departure waits for player fuel and repair");
   trip.Automatic=false;Check(!trip.DepartureDue(true,0,true,true,true) && trip.CanLeave(0),"manual schedule disables automatic departure only");
   var tripCopy=new ExpeditionModel();tripCopy.Restore(trip.Capture(),0,true,180);Check(tripCopy.Remaining==0 && !tripCopy.Automatic && tripCopy.HasRelay(0),"timer and mission flags survive restore");
   trip.Advance(0,true,1,180);Check(trip.Remaining==180 && trip.TimedStop==1 && !trip.DepartureDue(true,1,true,true,true),"next station starts a new timer and requires its own relay");
   for(int station=1;station<4;station++)trip.Collect(station);Check(!trip.Won(false) && trip.Won(true) && trip.Recovered==4,"victory requires all four relays and route end");
   var badTrip=trip.Capture();badTrip.remaining=float.NaN;bool tripRejected=false;try{tripCopy.Restore(badTrip,1,true,180);}catch(ArgumentException){tripRejected=true;}Check(tripRejected && tripCopy.TimedStop==0,"invalid schedule restore does not mutate mission");
   Check(!ExpeditionModel.Valid(new ExpeditionState{relayMask=0},1,false,180),"departed station cannot lose its required relay in a save");
   var aTrip=new ExpeditionModel();var bTrip=new ExpeditionModel();aTrip.Advance(60,true,0,180);for(int frame=0;frame<600;frame++)bTrip.Advance(.1f,true,0,180);Check(Math.Abs(aTrip.Remaining-bTrip.Remaining)<.01,"schedule independent of frame subdivision");
   var oldCrate=new WorldState{id="Station 1/Crate",active=true,looted=true};
   var oldWater=new WorldState{id="Station 1/Supply WaterBottle",active=false};
   var newShotgun=new WorldState{id="Train/Repair Workbench/Shotgun Pickup",active=true};
   var newShells=new WorldState{id="Station 1/Station House/Shell Pickup",active=true};
   var currentWorld=new[]{new WorldState{id=oldCrate.id},new WorldState{id=oldWater.id,active=true},newShotgun,newShells};
   var v1=new SaveData{schema=1,inventory=new InventoryState{parts=7},world=new[]{oldCrate}};
   Check(SaveMigration.Upgrade(v1,currentWorld) && v1.schema==3 && v1.hunger==100 && v1.thirst==100 && v1.inventory.parts==7 && v1.world[0].looted && v1.world[1].active,"schema one upgrades through supplies and mission without losing old loot");
   var v2=new SaveData{schema=2,stopIndex=2,parked=true,hunger=25,thirst=30,inventory=new InventoryState{ammo=20},world=new[]{oldCrate,oldWater}};
   Check(SaveMigration.Upgrade(v2,currentWorld) && v2.schema==3 && v2.hunger==25 && v2.thirst==30 && !v2.world[1].active && v2.world[2].active,"schema two keeps needs and collected supplies while adding shotgun loot");
   Check(v2.expedition.relayMask==3 && v2.expedition.timedStop==2 && v2.expedition.remaining==180 && ExpeditionModel.Valid(v2.expedition,2,true,180),"old departed stations credited and current dwell initialized");
   Check(v2.inventory.ammo==20 && !v2.inventory.shotgun && v2.inventory.tube==0 && v2.inventory.shells==0,"migration keeps pistol rounds without creating owned shotgun");
   var completeOld=new SaveData{schema=2,stopIndex=4,parked=true,inventory=new InventoryState(),world=new[]{oldCrate,oldWater}};
   Check(SaveMigration.Upgrade(completeOld,currentWorld) && completeOld.expedition.relayMask==15 && completeOld.expedition.timedStop==-1 && completeOld.expedition.remaining==0,"old completed route receives a valid completed mission");
   var missingOld=new SaveData{schema=2,inventory=new InventoryState(),world=new WorldState[0]};
   Check(!SaveMigration.Upgrade(missingOld,currentWorld) && missingOld.schema==2 && missingOld.world.Length==0,"migration refuses missing unrelated old entities before mutation");
   var duplicateOld=new SaveData{schema=2,inventory=new InventoryState(),world=new[]{oldCrate,oldCrate}};Check(!SaveMigration.Upgrade(duplicateOld,currentWorld),"migration rejects duplicate old IDs");
   var unknownOld=new SaveData{schema=2,inventory=new InventoryState(),world=new[]{new WorldState{id="Unknown"}}};Check(!SaveMigration.Upgrade(unknownOld,currentWorld),"migration rejects unknown old IDs");
   var currentTrip=v2.expedition;Check(SaveMigration.Upgrade(v2,currentWorld) && object.ReferenceEquals(currentTrip,v2.expedition) && v2.hunger==25,"schema three upgrade is idempotent and does not reset schedule");
   var rifle=new InventoryModel();Check(!rifle.TrySpendRifleRound() && rifle.ReloadRifle()==0,"unowned rifle cannot fire or reload");
   Check(rifle.TryTakeRifle() && !rifle.TryTakeRifle() && rifle.RifleMagazine==30 && rifle.SelectedWeapon==WeaponSlot.Rifle,"rifle ownership and initial magazine");
   rifle.TryAddRifleAmmo(45);for(int i=0;i<7;i++)rifle.TrySpendRifleRound();int rifleTotal=rifle.RifleAmmo+rifle.RifleMagazine;
   Check(rifle.ReloadRifle()==7 && rifle.RifleMagazine==30 && rifle.RifleAmmo+rifle.RifleMagazine==rifleTotal,"rifle reload conserves ammunition");
   for(int i=0;i<30;i++)rifle.TrySpendRifleRound();Check(!rifle.TrySpendRifleRound(),"empty rifle cannot fire");
   Check(!rifle.TryAddRifleAmmo(-1) && !rifle.IsValid(new InventoryState{rifleMagazine=1}) && !rifle.IsValid(new InventoryState{weapon=4}),"invalid rifle ammo and unowned equip rejected");
   var rifleCopy=new InventoryModel();rifleCopy.Restore(rifle.Capture());Check(rifleCopy.HasRifle && rifleCopy.RifleMagazine==0 && rifleCopy.RifleAmmo==38,"rifle inventory snapshot round trip");
   var aim=new AimModel();aim.Advance(.2f,true,true);Check(aim.Blend>.97f && aim.Blend<1,"aim enters smoothly rather than snapping");
   Check(aim.Spread(1.15f,.18f)<.21f && aim.Sensitivity<.52f && aim.Movement<.72f,"aim reduces spread sensitivity and movement");
   aim.Advance(.2f,false,true);Check(aim.Blend<.03f,"releasing aim smoothly returns to hip fire");
   aim.Advance(1,true,false);Check(aim.Blend<.0001f,"reload or disallowed state prevents aim");
   aim.Advance(1,true,true);aim.Reset();Check(aim.Blend==0 && aim.Sensitivity==1 && aim.Movement==1,"weapon switch inventory load and pause reset aim");
   aim.Advance(0,true,true);aim.Advance(float.NaN,true,true);aim.Advance(-1,true,true);Check(aim.Blend==0,"paused invalid and negative time cannot advance aim");
   var aimLong=new AimModel();var aimFrames=new AimModel();aimLong.Advance(.3f,true,true);for(int frame=0;frame<30;frame++)aimFrames.Advance(.01f,true,true);Check(Math.Abs(aimLong.Blend-aimFrames.Blend)<.00001,"aim transition is frame-subdivision independent");
   bool surfaces=true,periodic=true,normals=true;
   foreach(SurfaceStyle style in Enum.GetValues(typeof(SurfaceStyle)))for(int sample=0;sample<100;sample++){
    float u=sample*.0131f,v=sample*.0197f;var p=SurfacePattern.Sample(style,u,v);var repeat=SurfacePattern.Sample(style,u+1,v+1);
    surfaces&=p.r>=0 && p.r<=1 && p.g>=0 && p.g<=1 && p.b>=0 && p.b<=1 && p.height>=0 && p.height<=1 && p.metallic>=0 && p.metallic<=1 && p.smoothness>=0 && p.smoothness<=1;
    periodic&=Math.Abs(p.r-repeat.r)<.001 && Math.Abs(p.g-repeat.g)<.001 && Math.Abs(p.height-repeat.height)<.001;
    SurfacePattern.Normal(style,u,v,out float nx,out float ny,out float nz);normals&=Math.Abs(nx*nx+ny*ny+nz*nz-1)<.00001 && nz>0;
   }
   Check(surfaces,"procedural PBR channels stay in valid ranges");Check(periodic,"procedural materials repeat at texture boundaries");Check(normals,"procedural tangent normals are normalized and forward-facing");
   Console.WriteLine(checks+" standalone .NET core checks passed. These are not Unity tests.");
  }finally{Directory.Delete(root,true);}
 }
}
