using System;
using System.Collections.Generic;
using System.Linq;
using Discord;
using SoraBot_v2.Services;

namespace SoraBot_v2.Data.Entities.SubEntities
{
     public class Coord {
        public double lon { get; set; }
        public double lat { get; set; }
    }

    public class Weathercomand {
        public int id { get; set; }
        public string main { get; set; }
        public string description { get; set; }
        public string icon { get; set; }
    }

    public class Main {
        public double temp { get; set; }
        public float pressure { get; set; }
        public float humidity { get; set; }
        public double temp_min { get; set; }
        public double temp_max { get; set; }
    }

    public class Wind {
        public double speed { get; set; }
        public double deg { get; set; }
    }

    public class Sys {
        public int type { get; set; }
        public int id { get; set; }
        public double message { get; set; }
        public string country { get; set; }
        public double sunrise { get; set; }
        public double sunset { get; set; }
    }

    public class WeatherData
    {
        public Coord coord { get; set; }
        public List<Weathercomand> weather { get; set; }
        public Main main { get; set; }
        public int visibility { get; set; }
        public Wind wind { get; set; }
        public int dt { get; set; }
        public Sys sys { get; set; }
        public int id { get; set; }
        public string name { get; set; }
        public int cod { get; set; }

        public EmbedBuilder GetEmbed() =>
            new EmbedBuilder()
            .WithColor(Utility.PurpleEmbed)
            .WithAuthor(x => { x.Name = "OpenWeatherMap"; x.IconUrl =  ("https://pbs.twimg.com/profile_images/720298646630084608/wb7LSoAc.jpg"); })
            .AddField(x => x.WithName("Country 🌍").WithValue($"{name} , {sys.country}").WithIsInline(true))
            .AddField(x => x.WithName("Lat / Long 🗺").WithValue($"{coord.lat} / {coord.lon}").WithIsInline(true))
            .AddField(x => x.WithName("Condition 🌥️").WithValue(String.Join(", ", weather.Select(w => w.main))).WithIsInline(true))
            .AddField(x => x.WithName("Humidity ☔").WithValue($"{main.humidity}%").WithIsInline(true))
            .AddField(x => x.WithName("Wind Speed 🚩").WithValue($"{wind.speed} km/h").WithIsInline(true))
            .AddField(x => x.WithName("Temperature 🌡").WithValue($"{main.temp} °C").WithIsInline(true))
            .AddField(x => x.WithName("Min / Max Temp 🌡").WithValue($"{main.temp_min} °C / {main.temp_max} °C").WithIsInline(true));

    }
}