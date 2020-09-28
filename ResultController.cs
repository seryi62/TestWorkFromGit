using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResultController : MonoBehaviour
{
    //Действие, которое произведет контроллер с числами
    //Умножение
    //Деление
    //Разность
    //Сложение
    [Header("Условие блока =, >, <, >=, <=, Учет др событий")]
    public bool ROVNO = false;
    public bool MESHE = false;
    public bool BOLSH = false;
    public bool ROVNO_OR_BOLSHE = false;
    public bool ROVNO_OR_MENSHE = false;
    public bool EVENTS_UCHET = false;


    //Время ожидания открытия блока при отсутствии анимации
    [Header("Время открытия блока если у него нет Анимации")]
    public float WaitToOpenResultBlock = 3f;

    //Флаг, говорящий нам что блок сравнивает пришедшие числа
    [Header("Сравнить 2 последних числа по условию")]
    public bool SRAVNIT = false;

    //общее число чисел которое будет сравнивать блок прежде чем открыться, данная переменная позволяет сравнивать больше
    //2 чисел парами, т.е. пришло 1 число, ждем второе, пришло 2 - сравниваем с первым, пришло 3  - сравниваем 2 и 3 и т.п.
    [Header("Сколько всего чисел нужно сравнить")]
    public int MaxNumberCount = 2;

    //Счетчик приходящих чисел/событий
    private int NumberCount = 0;

    [Header("События на которые подписан блок")]
    //События на которые подписывается блок - это имена объектов от которых приходят Линии (так легче)
    public string[] MyEvents;

    [Header("Собыя которые создает блок при открытии")]
    //События, которые провоцирует блок после выполнения всех событий на которые он продписан - имя блока (так легче)
    public string[] MyActions;

    
    //Число, на подсказке блока - расчитываемое число
    [Header("Число на подсказке блока")]
    public float NumberExtencion;

    //Числа, пришедшие в блок в результате событий, от первого до последнего
    private List<float> Numbers = new List<float>();

    // Start is called before the first frame update
    void Start()
    {
        //Подписываемся на события, которые нам интересны
        for (int i = 0; i < MyEvents.Length; i++)
        {
            MyEventAgregator.World.AddListener(MyListeners, MyEvents[i].ToString());
        }
    }

    public void MyListeners(string EventName, float Labele)
    {
        //Озвучиваем событие прихода числа на РЕЗУЛЬТИРУЮЩИЙ КУБ
        MyEventAgregator.World.PublichEvent("NUMBER_IN_RESULT_CUBE");

        //События, которые провоцирует данный блок будут озвучены лиш после того как блок откроется и соотв
        //блок откроется если пришедшее в слушатель число удволетворяет его условию
        if (!SRAVNIT && ((Labele == NumberExtencion && ROVNO) ||
            (Labele < NumberExtencion && MESHE) ||
            (Labele > NumberExtencion && BOLSH) ||
            (Labele >= NumberExtencion && ROVNO_OR_BOLSHE) ||
        (Labele <= NumberExtencion && ROVNO_OR_MENSHE)))
        {
            //Если одно из условий выполнено, вызвать функцию открытия блока
            OpenBlock();
        }
        else if(SRAVNIT)
        {
            //если включен режим сравнения двух последних числел, то
            //Прибавляем одно число
            NumberCount++;

            //Запоминаем очередное пришедшее число если включен режим сравнения двух числе
            Numbers.Add(Labele);

            //Если числе набролось 2 и нам есть что сравнивать, то
            if (Numbers.Count == 2)
            {
                print("NUMBER[0] = " + Numbers[0] + " --- NUMBER[1] = " + Numbers[1]);

                if (Numbers[0] == Numbers[1] && ROVNO)
                    print("P1");

                if (Numbers[0] < Numbers[1] && MESHE)
                    print("P2");

                if (Numbers[0] > Numbers[1] && BOLSH)
                    print("P3");

                if (Numbers[0] >= Numbers[1] && ROVNO_OR_BOLSHE)
                    print("P4");

                if (Numbers[0] <= Numbers[1] && ROVNO_OR_MENSHE)
                    print("P5");

                //по выбранному условию сравниваем два последних пришедших числа
                if ((Numbers[0] == Numbers[1] && ROVNO) ||
                (Numbers[0] < Numbers[1] && MESHE) ||
                (Numbers[0] > Numbers[1] && BOLSH) ||
                (Numbers[0] >= Numbers[1] && ROVNO_OR_BOLSHE) ||
                (Numbers[0] <= Numbers[1] && ROVNO_OR_MENSHE))
                {
                    //зануляем массив числе
                    Numbers = new List<float>();

                    //Добавляем в него последнее пришедшее число, для его сравнения со следующим пришедшим числом
                    Numbers.Add(Labele);

                    //Открываем блок только если все числа пришедшие на блок прошли проверку, т.е. число проверенных пришедших чисел
                    //NumberCount равно количеству чисел которое долже проверить блок MaxNumberCount
                    if (NumberCount == MaxNumberCount)
                    {
                        //Открываем блок только если были сравнены все числа
                        OpenBlock();
                    }
                }
                else
                {
                    //Если не одно из условий не сработало - получаем ПОРАЖЕНИЕ
                    MyEventAgregator.World.PublichEvent("S_GAME_OVER");
                }
            } 
        }
        else if(EVENTS_UCHET)
        {
            //Если блок учитывает число произошедших событий из числа тех на которые он подписан, то
            //Прибавляем одно число
            NumberCount++;

            //Проверяем все ли события прослушаны
            if (NumberCount == MyEvents.Length)
            {
                //Если да, то открываем блок только если были сравнены все числа
                OpenBlock();
            }
        }
        else
        {
            //Если не одно из условий не сработало - получаем ПОРАЖЕНИЕ
            MyEventAgregator.World.PublichEvent("S_GAME_OVER");
        }
    }

    // Update is called once per frame
    private void OpenBlock()
    {
        //Озвучиваем событие открытия РЕЗУЛЬТИРУЮЩЕГО КУБА
        MyEventAgregator.World.PublichEvent("RESULT_CUBE_IS_OPEN");

        //Если у блока есть Аниматор запускаем Анимацию, если нет Анимации запускаем карантин открытия блока
        if (GetComponent<Animator>() != null)
            GetComponent<Animator>().enabled = true;
        else
            StartCoroutine(OpenBlockNoAnimation());
    }

    IEnumerator OpenBlockNoAnimation()
    {
        //Ждем время до открытия блока
        yield return new WaitForSeconds(WaitToOpenResultBlock);

        //Озвучиваем события которые провоцирует блок, в качестве второго элемента передается число, которое формируется
        //в результате расчета блоком
        for (int i = 0; i < MyActions.Length; i++)
        {
            //озвучиваем все события которые провоцирует блок
            MyEventAgregator.World.PublichEvent(MyActions[i].ToString(), NumberExtencion);
        }
    }

    public void AnimationEvent()
    {
        //все события которые озвучивает блок должны произойти только тогда, когда линия связи дойдет до следующего блока, т.е 
        //когда будет закончена анимация линии ссвязи - эта функция как раз срабатывает после анимации

        //Озвучиваем события которые провоцирует блок, в качестве второго элемента передается число, которое формируется
        //в результате расчета блоком
        for (int i = 0; i < MyActions.Length; i++)
        {
            //озвучиваем все события которые провоцирует блок
            MyEventAgregator.World.PublichEvent(MyActions[i].ToString(), NumberExtencion);
        }
    }
}
