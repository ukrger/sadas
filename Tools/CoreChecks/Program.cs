using System;
using System.IO;
using TrainSummer;
class Program {
 static int checks;
 static void Check(bool condition,string name){if(!condition)throw new Exception(name);checks++;Console.WriteLine("PASS "+name);}
 static void Main(){
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
   Console.WriteLine(checks+" standalone .NET core checks passed. These are not Unity tests.");
  }finally{Directory.Delete(root,true);}
 }
}
