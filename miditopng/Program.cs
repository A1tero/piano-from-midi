using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using Melanchall.DryWetMidi.Core;
using Melanchall.DryWetMidi.Interaction;
using Aspose.Drawing.Drawing2D;
using Aspose.Drawing;
using Aspose.Drawing.Imaging;
using Melanchall.DryWetMidi.Common;
using Melanchall.DryWetMidi.MusicTheory;
using Chord = Melanchall.DryWetMidi.Interaction.Chord;
using Note = Melanchall.DryWetMidi.Interaction.Note;

class Miditopng{
    private static string _songName = Console.ReadLine();
    private static string _songPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets/songs", _songName);
    private static string _violinKeyPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets/images/violinKey.png");
    
    private static MidiFile _midiFile = MidiFile.Read(_songPath);
    private static Image _violinKey = new Bitmap(_violinKeyPath);
    private static TempoMap _tempoMap = _midiFile.GetTempoMap();
    private static Image _wholeImage = new Bitmap(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets/images/notes/whole.png"));
    private static Image _quarterImage = new Bitmap(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets/images/notes/quarter.png"));
    private static Image _halfImage = new Bitmap(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets/images/notes/half.png"));
    private static Image _eighthImage = new Bitmap(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets/images/notes/eighth.png"));
    private static Image _sixteenthImage = new Bitmap(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets/images/notes/sixteenth.png"));
    
    private static Image _wholeRestImage = new Bitmap(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets/images/rests/whole.png"));
    private static Image _halfRestImage = new Bitmap(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets/images/rests/half.png"));
    private static Image _quarterRestImage = new Bitmap(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets/images/rests/quarter.png"));
    private static Image _eighthRestImage = new Bitmap(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets/images/rests/eighth.png"));
    private static Image _sixteenthRestImage = new Bitmap(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets/images/rests/sixteenth.png"));
    
    private static int _height = 200;
    private static int _width;
    private static float _pointer = 120;
    private static Pen _pen = new Pen(Color.Black, 2);
    private static Brush _brush = new SolidBrush(Color.Black);
    private static Graphics _g;
    
    private static Dictionary<NoteName, float> _notePositions = new Dictionary<NoteName, float>(){
        {new Note((SevenBitNumber)60).NoteName, -21f}, {new Note((SevenBitNumber)61).NoteName, -21f}, 
        {new Note((SevenBitNumber)62).NoteName, -14f}, {new Note((SevenBitNumber)63).NoteName, -14f}, 
        {new Note((SevenBitNumber)64).NoteName, -7f}, 
        {new Note((SevenBitNumber)65).NoteName, 0f}, {new Note((SevenBitNumber)66).NoteName, 0f}, 
        {new Note((SevenBitNumber)67).NoteName, 7f}, {new Note((SevenBitNumber)68).NoteName, 7f},
        {new Note((SevenBitNumber)69).NoteName, 14f}, {new Note((SevenBitNumber)70).NoteName, 14f}, 
        {new Note((SevenBitNumber)71).NoteName, 21f}
    };
    
    public static void Main(){
        List<Chord> chords = _midiFile.GetChords().Where(el => MinOctave(el.Notes) > 3).ToList();
        List<Rest> rests = RestsUtilities.GetRests(chords).ToList();
        var elements = new List<Tuple<string, string, long, TimedObjectsCollection<Note>>>(); 
        List<string> durations = new List<string>();
        TimedObjectsCollection<Note>? emptyCollection = null;
        chords.ForEach(el => elements.Add(new Tuple<string, string, long, TimedObjectsCollection<Note>>("chord", GetLength(el), el.Time, el.Notes)));
        rests.ForEach(el => elements.Add(new Tuple<string, string, long, TimedObjectsCollection<Note>>("chord", el.LengthAs<MusicalTimeSpan>(_tempoMap).ToString(),
            el.Time, emptyCollection)));
        elements.Sort((x, y) => x.Item3.CompareTo(y.Item3));
        _width = elements.Count * 60;
        var bmp = new Bitmap(_width, _height);
        _g = Graphics.FromImage(bmp);
        _g.SmoothingMode = SmoothingMode.AntiAlias;
        _g.InterpolationMode = InterpolationMode.HighQualityBicubic;
        _g.Clear(Color.White);
        
        DrawTrebleStaff();
        DrawViolinKey();

        foreach (var el in elements){
            if(el.Item1 == "chord") DrawChord(el.Item2, el.Item3, el.Item4);
            else DrawRest(el.Item2, el.Item3);
        }

        string resName = Slice(_songName, _songName.IndexOf("."));
        string resultPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets/results");
        if (!Directory.Exists(resultPath)) Directory.CreateDirectory(resultPath);
        resultPath = Path.Combine(resultPath, resName + ".png");
        bmp.Save(resultPath, ImageFormat.Png);
    }

    static void DrawChord(string length, long time,  TimedObjectsCollection<Note> notes){
        float x = _pointer;
        foreach (Note note in notes){
            float y = 135 + _notePositions[note.NoteName];
            DrawNote(length, x, y);
        }

        _pointer += 45;
    }

    static void DrawRest(string length, long time){
        float x = _pointer;
        float y = 121;
        float scale1 = 0.091f;
        float scale2 = 0.8f;
        switch (length){
            case "1/1": _g.DrawImage(_wholeRestImage, x, y + 20, _wholeImage.Width * scale1, _wholeImage.Height * scale1); break;
            case "1/2": _g.DrawImage(_halfRestImage, x, y,  _halfImage.Width * scale1, _halfImage.Height * scale1); break;
            case "1/4": _g.DrawImage(_quarterRestImage, x, y,  _quarterImage.Width * scale2, _quarterImage.Height * scale2); break;
            case "1/8": _g.DrawImage(_eighthRestImage, x, y,  _eighthImage.Width * scale2, _eighthImage.Height * scale2); break;
            case "1/16": _g.DrawImage(_sixteenthRestImage, x, y,  _sixteenthImage.Width * scale2, _sixteenthImage.Height * scale2); break;
        }
        _pointer += 45;
    }

    static void DrawNote(string length, float x, float y){
        float scale = 0.4f;
        y -= 40;
        switch (length){
            case "1/1": _g.DrawImage(_wholeImage, x, y + 20, _wholeImage.Width * scale, _wholeImage.Height * scale); break;
            case "1/2": _g.DrawImage(_halfImage, x, y,  _halfImage.Width * scale, _halfImage.Height * scale); break;
            case "1/4": _g.DrawImage(_quarterImage, x, y,  _quarterImage.Width * scale, _quarterImage.Height * scale); break;
            case "1/8": _g.DrawImage(_eighthImage, x, y,  _eighthImage.Width * scale, _eighthImage.Height * scale); break;
            case "1/16": _g.DrawImage(_sixteenthImage, x, y,  _sixteenthImage.Width * scale, _sixteenthImage.Height * scale); break;
        }
    }
    
    static void DrawTrebleStaff(){
        for (int i = 65; i <= 135; i += 14){
            _g.DrawLine(_pen, 20, i, _width, i);
        }
    }

    static void DrawViolinKey(){
        float scale = 0.17f;
        float newH = _violinKey.Height * scale;
        float newW =  _violinKey.Width * scale;
        _g.DrawImage(_violinKey, 45, 35, newW, newH);
    }

    static string GetLength(Chord chord){
        var time = chord.LengthAs<MusicalTimeSpan>(_tempoMap);
        return time.ToString();
    }

    static int MinOctave(TimedObjectsCollection<Note> notes){
        int res = 50;
        foreach (var note in notes){
            res = Math.Min(res, note.Octave);
        }
        return res;
    }

    static string Slice(string s, int endIndex){
        string res = "";
        for (int i = 0; i < endIndex; ++i) res += s[i];
        return res;
    }
}
