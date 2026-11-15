namespace TrainSummer {
public interface IInteractable {
 string Prompt(bool english);
 string Interact(TrainMotor train,ResourceInventory inventory,bool english);
}
}
