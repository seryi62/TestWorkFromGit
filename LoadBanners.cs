using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Android;
using UnityEngine.UI;
using System.IO;

public class LoadBanners : MonoBehaviour
{
    //Тип загружаемого баннера
    [Header("Тип баннера через запятую: BIG, MIN, POST")]
    public string TYPE_BANNER;

    //Извлечь один баннер из рекламной компании - один продвигаемый баннер - это самыйпервый баннер
    [Header("Загрузить первый баннер из списка")]
    public bool FirstBanner;

    //Извлечь один баннер в случайном порядке
    [Header("Загрузить один случайный баннер")]
    public bool OneRangeBanner;

    //Флаг позволяющий отключать скрипт вместе с покупкой "ОТКЛЮЧЕНИЯ РЕКЛАМЫ"
    [Header("Не показвать баннеры если вся реклама отключена")]
    public bool DisableWithAds;

    //Баннер контейнер - суда будут инстансироваться баннеры
    [Header("Контейнер для загужаемых баннеров")]
    public GameObject Conteiner;

    //Префаб для баннера - то что будет инстансировать скрипт
    [Header("Перефаб баннера")]
    public GameObject BannerPrefabs;

    //Лоадер, при наличии
    [Header("Лоадер, прогресс бар загрузки")]
    public GameObject Loader;

    //Лоадер, при наличии
    [Header("Окно ошибки, которое нужно показать при сетевой ошибке")]
    public GameObject LoadingError;

    //Текстура которая заменяет баннер в случае ошибки загрузки
    [Header("Текстура, исп в случае ошибки загрузки текстур")]
    public Texture2D ErrorTextures;


    public Text Information;

    //Массив куда заносятся данные о баннерах
    private Ads_info[] ADS_Banner_Information;

    //Массив предидущих версий баннеров
    private Dictionary<string, Ads_info> ADS_Banner_Last_Version = new Dictionary<string, Ads_info>();

    //Служебная функйия отладки - УДАЛИТЬ ПОТОМ
    public void ClireBannerInformation()
    {
        PlayerPrefs.SetString("ADS_BANNERS", "NON INFORMATION");
    }

    private void Awake()
    {
        //PlayerPrefs.DeleteAll();

        //Проверяем, есть ли в Плаер префс ключ и информация о баннерах
        if (!PlayerPrefs.HasKey("ADS_BANNERS"))
        {
            //Если нет  значит это первый запуск игры, создаем ключ и пишем в нее упоминание что информация отсутствует
            PlayerPrefs.SetString("ADS_BANNERS", "NON INFORMATION");
        }
    }


    void Start()
    {
        //Проверяем, если информация о баннерах уже была загружена в Плаер префс, то делаем ее копию в массив прошлых версий ADS_Banner_Last_Version
        if (PlayerPrefs.GetString("ADS_BANNERS") != "NON INFORMATION")
        {
            //Читаем из Плаер префс
            ADS_Banner_Information = JsonHelper.FromJson<Ads_info>(PlayerPrefs.GetString("ADS_BANNERS"));

            //Заносим информацию в массив
            for (int i = 0; i < ADS_Banner_Information.Length; i++)
            {
                ADS_Banner_Last_Version.Add(ADS_Banner_Information[i].BANNER, ADS_Banner_Information[i]);
            }
        }

        //Посылаем запрос на сервер для получения информации о рекламе, запрос не будет отправлен если тип баннера MIN и была выполнена покупка "ОТКЛЮЧЕНИЯ РЕКЛАМЫ"
        if (!(DisableWithAds && GameManager.SETTING_DATE.ADS_DISABLE))
            StartCoroutine(LoadTextFromServer(GameManager.URL_MY_SERVER_ADS_INFORMATION));
    }

    
    //Функция обновления сетевой информации
    public void ReffreshBannerInformation()
    {
        //Проверяем был ли передан объект "Окно с ошибкой сети", если да - прячем его
        if (LoadingError != null)
            LoadingError.SetActive(false);

        //Проверяем был ли передан объект "Лоадера", если да -  показываем его
        if (Loader != null)
            Loader.SetActive(true);


        //Посылаем запрос на сервер для получения информации о рекламе
        StartCoroutine(LoadTextFromServer(GameManager.URL_MY_SERVER_ADS_INFORMATION, true));
    }

    IEnumerator BannerMoved()
    {
        //Данная функция перемешивает все баннеры
        //Создаем генератор случайных чисел и запоминаем число баннеров
        System.Random rn = new System.Random();
        int Number = ADS_Banner_Information.Length - 1;

        //Начинаем цикл перемешивания, заметим что если поставлен флаг вернуть только один первый баннер - перемешивание не происходит
        while (Number > 0 && !FirstBanner)
        {
            //Принцип перемешивания:
            //Производим перемену значений ячеек, причем берем всегда последнюю ячейку,
            //записываем в нее случайно выбранное значение из случайной ячейки, в которую
            //записываем значение из последней ячейки. Т.О. последняя ячейка получает
            //значение из середины массива, при следующей же итерации последняя ячейка уже не будет
            //задействована и ее значение не поменяется.

            //получаем число на интервале от 1 до Number
            int j = rn.Next(0, Number + 1);
            //Запоминаем значение ячейки с индексом Number
            Ads_info Banner = ADS_Banner_Information[Number];
            //Записываем в ячейку с индексом Number значение ячейки с индексом J
            ADS_Banner_Information[Number] = ADS_Banner_Information[j];
            //в ячейку с индексом J записываем запомненное значение из ячейки с индексом Number
            ADS_Banner_Information[j] = Banner;

            //Уменьшаем число итераций
            Number--;

            //Передаем управление
            yield return new WaitForSeconds(0.05f);
        }
    }


    //Загрузка теста с сервера
    IEnumerator LoadTextFromServer(string url, bool Wait = false)
    {
        //Если передан аргумент wait  - ждем три секунды
        if(Wait)
            yield return new WaitForSeconds(3f);

        //Принимает аргументы: URL для запроса, параметр задержки запроса wait
        //Формируем и посылаем Гет запрос (в него входят: URL, Язык используемый в приложении, имя пакета приложения, чтобы сервер мог понять что это за приложение)
        var request = UnityWebRequest.Get(url + "?language=" + GameManager.SETTING_DATE.THIS_LANGUAGE + "&Application=" + Application.identifier);

        //передаем упр пока не придет ответ от сервера
        yield return request.SendWebRequest();

        //Проверяем наличие ошибок
        if (!request.isHttpError && !request.isNetworkError)
        {
            //Если ошибок нет
            //Сохраняем информацию в PlayerPrefs
            PlayerPrefs.SetString("ADS_BANNERS", request.downloadHandler.text);

            //Расшифровывем полученную от сервера информацию о текущих актуальных баннераx и переводим ее в массив баннероа
            ADS_Banner_Information = JsonHelper.FromJson<Ads_info>(request.downloadHandler.text);

            //Запускаем каранти перемещивания баннеров
            yield return StartCoroutine(BannerMoved());

            //Переменная хранящая загруженную текстуру
            Texture2D Tex;

            //Перебираем весь новый массив баннеров
            for (int i = 0; i < ADS_Banner_Information.Length; i++)
            {
                //Обнуляем текстуру в начале цикла
                Tex = null;

                //Проверяем соответствия старой и новойверсии баннера
                if (!ADS_Banner_Last_Version.ContainsKey(ADS_Banner_Information[i].BANNER) || !System.IO.File.Exists(Path.Combine(Application.persistentDataPath, ADS_Banner_Information[i].BANNER + ".png")) || (ADS_Banner_Last_Version.ContainsKey(ADS_Banner_Information[i].BANNER) ? ADS_Banner_Information[i].BANNER_VERSION_ADS != ADS_Banner_Last_Version[ADS_Banner_Information[i].BANNER].BANNER_VERSION_ADS : true))
                {
                    print("ВЗАИМОДЕЙСТВИЕ С СЕТЬЮ");

                    //Запрос на загрузку текстуры посылается в трех случаях:
                    //1 - если загруженного банера нет в списке старых баннеров
                    //2 - если файл тексты небыл загружен по какой то причине
                    //3 - если версия нового баннера не совпадает со старой версией того же баннера

                    //Формируем и посылаем Гет запрос
                    var requestTex = UnityWebRequestTexture.GetTexture(ADS_Banner_Information[i].URL_ICON_ADS);

                    //передаем упр пока не придет ответ от сервера
                    yield return requestTex.SendWebRequest();

                    //Проверяем наличие ошибок
                    if (!requestTex.isHttpError && !requestTex.isNetworkError)
                    {
                        //Сохраняем текстуру в файл, переведя ее в байты
                        if(Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite))
                            File.WriteAllBytes(Path.Combine(Application.persistentDataPath, ADS_Banner_Information[i].BANNER + ".png"), DownloadHandlerTexture.GetContent(requestTex).EncodeToPNG());

                        //Сохраняем текстуру в промежуточную переменную
                        Tex = DownloadHandlerTexture.GetContent(requestTex);
                    }
                    else
                    {
                        //В случае ошибки - логируем
                        Debug.LogErrorFormat("error request [{0}, {1}]", ADS_Banner_Information[i].URL_ICON_ADS, requestTex.error);
                    }

                    //Очишаем запрос
                    requestTex.Dispose();
                }
                
                //вызываем функцию создания баннера
                if (!CreatBanner(ADS_Banner_Information[i], Tex))
                    continue;

                //Если нам требуется только один банер, случайный или первый, то прерываем цикл после первой итерации
                if (OneRangeBanner || FirstBanner)
                    break;

                //Передаем управление
                yield return new WaitForSeconds(0.05f);
            }
        }
        else
        {
            //В случае ошибки - логируем
            Debug.LogErrorFormat("error request [{0}, {1}]", url, request.error);

            //запускаем карантин
            //Проверяем была ли загружена информация из сети, если была, значит есть и иконки и можно запускать загрузку из памяти
            if (PlayerPrefs.GetString("ADS_BANNERS") != "NON INFORMATION")
            {
                //Читаем из Плеер префс
                ADS_Banner_Information = JsonHelper.FromJson<Ads_info>(PlayerPrefs.GetString("ADS_BANNERS"));

                //Запускаем карантин перемешивания баннеров
                yield return StartCoroutine(BannerMoved());

                //Перебираем весь новый массив баннеров и выводим их в контейнер
                for (int i = 0; i < ADS_Banner_Information.Length; i++)
                {
                    print("ЗАГРУЗКА ИЗ ПАМЯТИ ПРИ ОТСУТСТВИИ СЕТЕВОГО ПОДКЛ.");

                    //Вызываем функцию создания баннера
                    if (!CreatBanner(ADS_Banner_Information[i]))
                        continue;

                    //Если нам требуется только один банер, случайный или первый, то прерываем цикл после первой итерации
                    if (OneRangeBanner || FirstBanner)
                        break;

                    yield return new WaitForSeconds(0.05f);
                }
            }
            else
            {
                //Проверяем был ли передан объект "Окно с ошибкой сети"
                //если да -показываем его, заметим что окошко показывается лишь в случае если загрузка баннеров из памяти неудалась
                if (LoadingError != null)
                    LoadingError.SetActive(true);
            }
        }

        //Проверяем был ли передан объект "Лоадера", если да -прячем лоадер
        if (Loader != null)
            Loader.SetActive(false);

        //Очишаем запрос
        request.Dispose();
    }


    //Данная функция служит для создания баннера. возвращает 1 если показ удачный, 0 - не удачный.
    private bool CreatBanner(Ads_info Info, Texture2D Tex = null)
    {
        //Сразу проверяем соответствует ли тип баннера, который требуется отобразить типу который отображает скрипт
        if (TYPE_BANNER.Contains(Info.TYPE))
        {
            //Если текстура не передана загружаем ее из памяти, если текстура иконки передана исп ее
            string Origin = Path.Combine(Application.persistentDataPath, Info.BANNER + ".png");
            
            if (System.IO.File.Exists(Origin) && Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead))
            {
                //Если файл существует загружаем его
                Tex = new Texture2D(1, 1);
                Tex.LoadImage(System.IO.File.ReadAllBytes(Origin));
            }
            else if(Tex == null)
            {
                //Если файл иконки не существует, используем текстуру "Ошибку"
                Tex = ErrorTextures;
            }

            //Контейнер префаба баннера
            GameObject Banner;
            Banner = Instantiate(BannerPrefabs);
            Banner.transform.SetParent(Conteiner.transform, false);

            //Вешаем на его скрипт Ссылку для перехода при клике по баннеру и добавляем иконку (ЗАМЕТИМ ЧТО ССЫЛКА И ИКОНКА ЭТО ОБЪЩИЕ ЭЛЕМЕНТЫ У ВСЕХ ТИПОВ БАННЕРОВ)
            Banner.GetComponent<ClicToBanner>().URL_TO_ANCHOR = Info.URL_REFERRER;
            Banner.transform.Find("Icon").GetComponent<Image>().sprite = Sprite.Create(Tex, new Rect(0, 0, Tex.width, Tex.height), new Vector2(0.5f, 0.5f));

            //В соответствии с типом баннера вносим в его поля информацию
            //BIG_BANNER - содержит и название и описание
            //MIN_BANNER - содержит только название
            switch (Info.TYPE)
            {
                case "BIG":
                    Banner.transform.Find("Coment").GetComponent<Text>().text = Info.TEXT_ADS;
                    Banner.transform.Find("Name").GetComponent<Text>().text = Info.TITLE;
                    break;
                case "MIN":
                    Banner.transform.Find("Name").GetComponent<Text>().text = Info.TITLE;
                    break;
            }

            return true;
        }

        return false;
    }
}


//ДАННЫЙ КЛАСС НА ДАННЫЙ МОМЕНТ РЕАЛИЗОВАН В СКРИПТЕ LOADER
[Serializable]
public class Ads_info
{
    public string BANNER;
    public string TYPE;
    public string TITLE;
    public string BANNER_VERSION_ADS;
    public string URL_REFERRER;
    public string URL_ICON_ADS;
    public string TEXT_ADS;
}