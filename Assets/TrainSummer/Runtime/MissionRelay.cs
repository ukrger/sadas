using UnityEngine;
namespace TrainSummer {
// State belongs to ExpeditionSession, never to a pooled model or a second persistent ID.
public sealed class MissionRelay:MonoBehaviour,IInteractable {
 public ExpeditionSession mission;public int stationIndex;
 public string Prompt(bool english)=>mission.Model.HasRelay(stationIndex)?(english?"Relay recovered":"Модуль уже забран"):(english?"E — recover the radio relay":"E — забрать радиомодуль");
 public string Interact(TrainMotor train,ResourceInventory inventory,bool english)=>mission.Collect(stationIndex)?(english?"Radio relay recovered.":"Радиомодуль найден."):(english?"This relay was already recovered.":"Этот модуль уже забран.");
}
}
