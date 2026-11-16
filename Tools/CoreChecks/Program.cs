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
   Console.WriteLine(checks+" standalone .NET core checks passed. These are not Unity tests.");
  }finally{Directory.Delete(root,true);}
 }
}
