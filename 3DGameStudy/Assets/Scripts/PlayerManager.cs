using UnityEngine; // NameSpace : �Ҽ�

class Item
{
    public virtual void Eat()
    {
        Debug.Log("�׳� �Դ�");
    }
}

class Potion : Item
{
    public override void Eat()
    {
        Debug.Log("������ �Դ�");
    }

    public void Heal()
    {
        Debug.Log("��!");
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
        // for�� ����
        for (int i = 2; i < 10; i++)
        {
            for(int j = 1; j < 10; j++)
            {
                num[k] = i * j; // ����
                Debug.Log($"{i} X {j} = {num[k]}"); // ���
                k ++;
            }
        }

        // �������̵� ����
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
