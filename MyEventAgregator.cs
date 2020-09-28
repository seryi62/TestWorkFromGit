using System;
using System.Collections.Generic;
using UnityEngine;

public class MyEventAgregator
{
    //Статическое поле класса для доступа к Агрегатору
    public static WorldEvent World = new WorldEvent();

    //Публичный статик метод очищающий агрегатор - вызов метода обязателен, когда сессия использования
    //агрегатора закончилась, например, один уровень закончился и начался другой
    public static void ResetALL()
    {
        World.Cleare();
    }
}

public class WorldEvent
{
    //Класс Агрегатора
    //Список, состоящий из объектов, хранящих информацию о подписчике
    private List<MyListenerOneArguments> MyListenerOneArg = new  List<MyListenerOneArguments>();
    private List<MyListenerNoArguments> MyListenerNoArg = new List<MyListenerNoArguments>();
    private List<MyListenerNoArguments> MyListenerDondCleare = new List<MyListenerNoArguments>();

    //Метод подписки на событие. Он принимает наш Делегат - это функция без аргументово
    //Метод забивает в агрегатор единоразовые постоянные подписки
    public void AddListenerDontCleare(Action<string> callback, string EventName)
    {
        //Данный метод занимается добавлением Делегатов подписчиков в Агрегатор

        //Выводим в консоль
        Debug.Log("Set Listener - EVENT = " + EventName);

        //Добавляем слушателя событий в лист - создаем объект с данными, передаем в него Делегат
        //и имя события на которое срабатывает данный делегат.
            MyListenerDondCleare.Add(new MyListenerNoArguments(callback, EventName));
    }

    //Метод подписки на событие. Он принимает наш Делегат - это функция с 1 н аргумент float - это число, которое
    //передает один блок другому блоку и вторым аргументом - имя собыития при возникновении которого Делегат срабатывает.
    public void AddListener(Action<string, float> callback, string EventName, bool Dont_Cleare = false)
    {
        //Данный метод занимается добавлением Делегатов подписчиков в Агрегатор

        //Выводим в консоль
        //Debug.Log("Set Listener - EVENT = " + EventName);

        //Добавляем слушателя событий в лист - создаем объект с данными, передаем в него Делегат
        //и имя события на которое срабатывает данный делегат.
        MyListenerOneArg.Add(new MyListenerOneArguments(callback, EventName, Dont_Cleare));
    }

    public void AddListener(Action<string> callback, string EventName, bool Dont_Cleare = false)
    {
        //Данный метод занимается добавлением Делегатов подписчиков в Агрегатор

        //Выводим в консоль
        //Debug.Log("Set Listener - EVENT = " + EventName);

        //Добавляем слушателя событий в лист - создаем объект с данными, передаем в него Делегат
        //и имя события на которое срабатывает данный делегат.
        MyListenerNoArg.Add(new MyListenerNoArguments(callback, EventName, Dont_Cleare));
    }


    public void PublichEvent(string EventName, float Labele = 0)
    {
        Debug.Log("EVENT = " + EventName);

        //Данный метод слушает события и вызывает Делегат подписчик в соответствии с событием в качестве аргументов
        //он принимает имя события которое произошло и число которое нужно передать Делегату, если событие
        //не предусматривает передачу числа, то его передавать не обязательно

        //В первом цикле перебираем всех подписчиков принимающих число
        foreach (MyListenerOneArguments GetListenerOneArg in MyListenerOneArg)
        {
            //Проверяем, есть ли у нас в массиве объект, содержащий Делегат, подписанный на событие EventName
            if (GetListenerOneArg.MyEvent == EventName)
            {
                //Выводим в консоль
                Debug.Log("Get Listener = " + EventName);

                //Если, Делегат есть, вызываем его передав число
                GetListenerOneArg.MyAction(EventName, Labele);
            }
        }

        
        //В втором цикле перебираем всех подписчиков НЕ принимающих число
        foreach (MyListenerNoArguments GetListenerNoArg in MyListenerNoArg)
        {
            //Проверяем, есть ли у нас в массиве объект, содержащий Делегат, подписанный на событие EventName
            if (GetListenerNoArg.MyEvent == EventName)
            {
                //Выводим в консоль
                Debug.Log("Get Listener DontCleare = " + EventName);

                //Если Делегат есть, вызываем его передав число
                GetListenerNoArg.MyAction(EventName);
            }
        }

        //В втором цикле перебираем всех подписчиков НЕ принимающих число
        foreach (MyListenerNoArguments GetListenerNoArg in MyListenerDondCleare)
        {
            //Проверяем, есть ли у нас в массиве объект, содержащий Делегат, подписанный на событие EventName
            if (GetListenerNoArg.MyEvent == EventName)
            {
                //Выводим в консоль
                Debug.Log("Get Listener DontCleare = " + EventName);

                //Если Делегат есть, вызываем его передав число
                GetListenerNoArg.MyAction(EventName);
            }
        }
    }

    //Метод сброса агрегатора
    public void Cleare()
    {
        MyListenerOneArg = new List<MyListenerOneArguments>();
        MyListenerNoArg = new List<MyListenerNoArguments>();
    }
}


//Данный клас описывает объект с данными для делегата
public struct MyListenerOneArguments
{
    //Переменная для хранения делегата и переменная с именем события при возникновении которого он срабатывает
    public Action<string, float> MyAction;
    public string MyEvent;

    //Флаг говорящий о том, что собыие подлежит очистке или не должно быть очищенео
    public bool DONT_CLEARE;

    //Это конструктор объекта
    public MyListenerOneArguments(Action<string, float> _MyAction, string _MyEvent, bool Dont_Cleare = false)
    {
        //Запоминаем делегат и событие на которе он подписан и при появлении которого должен сработать
        MyAction = _MyAction;
        MyEvent = _MyEvent;
        DONT_CLEARE = Dont_Cleare;
    }
}

//Данный клас описывает объект с данными для делегата
public struct MyListenerNoArguments
{
    //Переменная для хранения делегата и переменная с именем события при возникновении которого он срабатывает
    public Action<string> MyAction;
    public string MyEvent;

    //Флаг говорящий о том, что собыие подлежит очистке или не должно быть очищенео
    public bool DONT_CLEARE;

    //Это конструктор объекта
    public MyListenerNoArguments(Action<string> _MyAction, string _MyEvent, bool Dont_Cleare = false)
    {
        //Запоминаем делегат и событие на которе он подписан и при появлении которого должен сработать
        MyAction = _MyAction;
        MyEvent = _MyEvent;
        DONT_CLEARE = Dont_Cleare;
    }
}
