using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;



public class EventController : MonoBehaviour, IPointerClickHandler
{
    //Действие, которое произведет контроллер с числами
    //Умножение
    //Деление
    //Разность
    //Сложение
    [Header("Стартовая операция блока X / + -")]
    public bool MULT = false;
    public bool DELL = false;
    public bool DIFF = false;
    public bool SUMM = false;


    [Header("Открыть только если блок услышал все события")]
    //Данная переменная говорит нам что блок будет открыт после того как ьблок ушлышит все события вне зависимости от
    //того какое число указано на подсказке и в переменной NumberExtencion
    public bool OpenBlock;

    [Header("События на которые подписан блок")]
    //События на которые подписывается блок - это имена объектов от которых приходят Линии (так легче)
    public string[] MyEvents;

    [Header("Собыя которые создает блок при открытии")]
    //События, которые провоцирует блок после выполнения всех событий на которые он продписан - имя блока (так легче)
    public string[] MyActions;


    //Число, на подсказке блока - расчитываемое число
    [Header("Число на подсказке блока")]
    public float NumberExtencion;

   

    //Переменная хранящая промежуточный расчет
    private float Result = 0;

    //Счетчик событий на которые подписан блок - считает число выполненных событий, на которые подписан блок
    private int MyCountEvent = 0;

    //Числа, пришедшие в блок в результате событий, от первого до последнего
    private List<float> Numbers = new List<float>();

    //Ссылки на активные поля блока:
    //действие которое выполняет блок и число блока
    [Header("Обязательные поля с операцией и подсказкой")]
    public GameObject TextBrain;
    public Text TextNumber;

    void Start()
    {
        //Подписываемся на события, которые нам интересны
        for (int i = 0; i < MyEvents.Length; i++)
        {
            MyEventAgregator.World.AddListener(MyListeners, MyEvents[i].ToString());
        }
    }


    //Обеспечиваем смену знака блока при клике
    public void OnPointerClick(PointerEventData eventData)
    {
        //Озвучиваем событие клика по СЧЕТНОМУ КУБУ
        MyEventAgregator.World.PublichEvent("SCHET_CUBE_IS_CLICK");

        //Умножение, деление, разность, сумма
        if (MULT)
        {
            MULT = false;
            DELL = true;
            TextBrain.GetComponent<Text>().text = "/";
        }
        else if (DELL)
        {
            DELL = false;
            DIFF = true;
            TextBrain.GetComponent<Text>().text = "-";
        }
        else if (DIFF)
        {
            DIFF = false;
            SUMM = true;
            TextBrain.GetComponent<Text>().text = "+";
        }
        else if(SUMM)
        {
            SUMM = false;
            MULT = true;
            TextBrain.GetComponent<Text>().text = "X";
        }
    }

    public void MyListeners(string EventName, float Labele)
    {
        //Озвучиваем событие прихода числа на СЧЕТНЫЙ КУБ
        MyEventAgregator.World.PublichEvent("NUMBER_IN_SCHET_CUBE");

        //Функция слушателя события на которые подписан блок - слушает все события от первого до последнего, на которые мы подписаны
        //Запоминаем очередное пришедшее число
        Numbers.Add(Labele);

        //Действие, которое произведет контроллер с числами
        //Умножение всех чисел между собой, Деление первого полученного числа на все последующие по очереди,
        //Разность первого числа и всех последующих, Сложение всех чисел
        if (MULT)
        {
            //Премножаем все числа
            Result = 1;
            for(int i = 0; i< Numbers.Count; i++)
            {
                Result *= Numbers[i];
            }
        }
        else if(DELL)
        {
            //Делим первое число на все последующие по очереди
            Result = Numbers[0];
            if (Numbers.Count >= 2)
            {
                for (int i = 1; i < Numbers.Count; i++)
                {
                    Result /= Numbers[i];
                }
            }
        }
        else if(DIFF)
        {
            //Вычитаем все числа из первого полученного
            Result = Numbers[0];
            if (Numbers.Count >= 2)
            {
                for (int i = 1; i < Numbers.Count; i++)
                {
                    Result -= Numbers[i];
                }
            }
        }
        else if(SUMM)
        {
            //Складываем все числа
            Result = 0;
            foreach (float Val in Numbers)
            {
                Result += Val;
            }
        }
        else
        {
            //если действие небыло выбрано пришедшее число равно числу блока
            Result = NumberExtencion;
        }

        //эта функция выполняется каждый раз когда выполняется интересное для нас событие на которое мы подписаны
        //Прибавляем счетчик событий
        MyCountEvent++;

        //События, которые провоцирует данный блок будут озвучены лиш после того как блок откроется и соотв
        //расчитываемое в блоек число будет равно числу на подсказке блока, это означает открытие блока и отправку числа далее
        if ((Result == NumberExtencion && !OpenBlock) || (OpenBlock && MyCountEvent == MyEvents.Length))
        {
            //Передаем полученное число на текстовое поле блока, если блок открытый и 
            //подменяем им число блока
            if (OpenBlock)
            {
                //подменяем число
                NumberExtencion = Result;

                //отображаем результат - подменяя визуал, оставляем только 2 знака если число дробное
                if (Math.Truncate(Result) == 0 && Result != 0 && Result.ToString().Length >= 3)
                    TextNumber.text = Result.ToString().Substring(0, 3);
                else
                    TextNumber.text = Result.ToString();
            }

            //если число расчиталось правильно - запускаем анимацию блока
            GetComponent<Animator>().enabled = true;

            //Полность делаем не прозрачной подсказку и скрываем знак математического дейтвия на блоке
            TextNumber.color = new Color(TextNumber.color.r, TextNumber.color.g, TextNumber.color.b, 1);
            TextBrain.SetActive(false);

            //Озвучиваем событие открытия СЧЕТНОГО КУБА
            MyEventAgregator.World.PublichEvent("SCHET_CUBE_IS_OPEN");
        }
        else if (MyCountEvent == MyEvents.Length && Result != NumberExtencion && !GetComponent<Animator>().enabled)
        {
            //В случае если все события на которые подписан блок произошли, а анимация блока так и не была запущена и
            //при всем при этом расчитанное число не равно числу на подсказке, то озвучиваем событие ПРОИГРЫША
            //объясним почему - так как все события на которые подписан блок прозвучали, а расчитанное число так и не равно
            //числу на подсказке, то расчитанное число уже никак не изменить и соотв не открыть блок, а значит и нельзя
            //продолжить цепочку линий связи и блоко, что автоматически не позволит нам дойти до результирующего блока и открыть его,
            //а значит и выиграть уровень...
            MyEventAgregator.World.PublichEvent("S_GAME_OVER");
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
