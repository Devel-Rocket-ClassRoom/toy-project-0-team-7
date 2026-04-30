using UnityEngine;
public enum StationType   //하늘님 스크립트와 합친 후 삭제 예정
{
    Circle,
    Triangle,
    Square,
}
public enum PassengerState
{
    Waiting,
    OnTrain,
    Arrived,
}
public class Passenger
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

