using UnityEngine; // NameSpace : 소속

class Item
{
    public virtual void Eat()
    {
        Debug.Log("그냥 먹다");
    }
}

class Potion : Item
{
    public override void Eat()
    {
        Debug.Log("포션을 먹다");
    }

    public void Heal()
    {
        Debug.Log("힐!");
    }
}

class Cookie : Item
{

}


public class PlayerManager : MonoBehaviour
{
    int[] num = new int[100];
    int k = 0;
    Potion potion = new Potion();
    Item item = new Item();
    Item potion2 = new Potion();
    Cookie cookie = new Cookie();

    void Start()
    {
        // for문 예제
        for (int i = 2; i < 10; i++)
        {
            for(int j = 1; j < 10; j++)
            {
                num[k] = i * j; // 저장
                Debug.Log($"{i} X {j} = {num[k]}"); // 출력
                k ++;
            }
        }

        // 오버라이드 예제
        item.Eat();
        potion.Eat();
        potion2.Eat();
        cookie.Eat();

        potion.Heal();
    }

    void Update()
    {
        
    }
}
