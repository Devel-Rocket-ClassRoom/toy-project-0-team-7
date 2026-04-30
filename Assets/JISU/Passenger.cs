using UnityEngine;
public class Passenger : MonoBehaviour
{
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
    //가고싶은 역
    public StationType destination { get; private set; }


}

