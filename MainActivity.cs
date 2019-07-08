using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Runtime;
using Android.Widget;
using System.Net.Http;
using System;
using Newtonsoft.Json.Linq;
using System.Globalization;
using WeatherApp.Fragments;
using System.Net;
using System.IO;
using Android.Graphics;
using Plugin.Connectivity;

namespace WeatherApp
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        Button getWeatherButton;
        TextView placeTextView;
        TextView temperatureTextView;
        TextView weatherDescriptionTextView;
        EditText cityNameEditText;
        ImageView weatherImageView;

        ProgressDialogueFragment progressDialogue;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);

            cityNameEditText = (EditText)FindViewById(Resource.Id.cityNameText);
            placeTextView = (TextView)FindViewById(Resource.Id.placeText);
            temperatureTextView = (TextView)FindViewById(Resource.Id.temperatureTextView);
            weatherDescriptionTextView = (TextView)FindViewById(Resource.Id.weatherDescriptionText);
            weatherImageView = (ImageView)FindViewById(Resource.Id.weatherImage);
            getWeatherButton = (Button)FindViewById(Resource.Id.getWeatherButton);

            getWeatherButton.Click += GetWeatherButton_Click;
            GetWeather("London");
        }

        private void GetWeatherButton_Click(object sender, System.EventArgs e)
        {
            string place = cityNameEditText.Text;
            GetWeather(place);
            cityNameEditText.Text = "";
        }

       async  void GetWeather(string place)
        {
            string apiKey = "bae28df78f81c27a2be7a7e3f1ef3c4e";
            string apiBase = "https://api.openweathermap.org/data/2.5/weather?q=";
            string unit = "metric";

            if (string.IsNullOrEmpty(place))
            {
                Toast.MakeText(this, "please enter a valid city name", ToastLength.Short).Show();
                return;
            }

            if (!CrossConnectivity.Current.IsConnected)
            {
                Toast.MakeText(this, "No internet connection", ToastLength.Short).Show();
                return;
            }

            ShowProgressDialogue("Fetching weather...");

            // Asynchronous API call using HttpClient
            string url = apiBase + place + "&appid=" + apiKey + "&units=" + unit;
            var handler = new HttpClientHandler();
            HttpClient client = new HttpClient(handler);
            string result = await client.GetStringAsync(url);

            Console.WriteLine(result);

            var resultObject = JObject.Parse(result);
            string weatherDescription = resultObject["weather"][0]["description"].ToString();
            string icon = resultObject["weather"][0]["icon"].ToString();
            string temperature = resultObject["main"]["temp"].ToString();
            string placename = resultObject["name"].ToString();
            string country = resultObject["sys"]["country"].ToString();
            weatherDescription = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(weatherDescription);

            weatherDescriptionTextView.Text = weatherDescription;
            placeTextView.Text = placename + ", " + country;
            temperatureTextView.Text = temperature;


            // Download Image using WebRequest
            string ImageUrl = "http://openweathermap.org/img/w/" + icon + ".png";
            System.Net.WebRequest request = default(System.Net.WebRequest);
            request = WebRequest.Create(ImageUrl);
            request.Timeout = int.MaxValue;
            request.Method = "GET";

            WebResponse response = default(WebResponse);
            response = await request.GetResponseAsync();
            MemoryStream ms = new MemoryStream();
            response.GetResponseStream().CopyTo(ms);
            byte[] imageData = ms.ToArray();

            Bitmap bitmap = BitmapFactory.DecodeByteArray(imageData, 0, imageData.Length);
            weatherImageView.SetImageBitmap(bitmap);

           
            ClossProgressDialogue();
        }


        void ShowProgressDialogue(string status)
        {
            progressDialogue = new ProgressDialogueFragment(status);
            var trans = SupportFragmentManager.BeginTransaction();
            progressDialogue.Cancelable = false;
            progressDialogue.Show(trans, "progress");
        }

        void ClossProgressDialogue()
        {
            if(progressDialogue != null)
            {
                progressDialogue.Dismiss();
                progressDialogue = null;
            }
        }
    }
}