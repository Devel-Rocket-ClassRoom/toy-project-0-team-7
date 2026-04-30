using UnityEngine;

public enum PassengerState
{
    Waiting,
    OnTrain,
    Arrived,
}
public class Passenger
{
    //가고싶은 역
    public StationType destination { get; private set; }
    //승객 상태
    private PassengerState state;
    public PassengerState State
    {
        get => state;
        set
        {
            state = value;
            Debug.Log($"Passenger state changed to: {state}");
        }
    }
  
    public Passenger(StationType dest)
    {
        this.destination = dest;
        this.state = PassengerState.Waiting;
    }

}

