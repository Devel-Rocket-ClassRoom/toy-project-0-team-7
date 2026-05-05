using UnityEngine;

public class Passenger : MonoBehaviour
{
    //가고싶은 역
    public StationType destination { get; private set; }

    public Station transferStation;

    public int blockedLineId = -1;

    //승객 상태
    private PassengerState state;
    public PassengerState State
    {
        get => state;
        set
        {
            state = value;
            //Debug.Log($"Passenger state changed to: {state}");
        }
    }
  
    public void Init(StationType dest)
    {
        destination = dest;
        state = PassengerState.Waiting;
    }
}