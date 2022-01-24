using Godot;
using System;
using NWaves.Features;
using NWaves.Signals;
using NWaves.Audio;
using System.Collections.Generic;
using System.Linq;

public class Test : Node2D
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";

    // Called when the node enters the scene tree for the first time.
    private AudioEffectRecord _effect;
    private AudioStreamSample _recording;

    private float interval = 0.1f;
    private float delta_count = 0f;

    private int recordingIndex = 0;

    public override void _Ready()
    {
        // We get the index of the "Record" bus.
        int idx = AudioServer.GetBusIndex("Record");
        // And use it to retrieve its first effect, which has been defined
        // as an "AudioEffectRecord" resource.
        _effect = (AudioEffectRecord)AudioServer.GetBusEffect(idx, 0);
    }

    public void OnRecordButtonPressed()
    {
        if (_effect.IsRecordingActive())
        {
            GetNode<Button>("SaveButton").Disabled = false;
            _effect.SetRecordingActive(false);
            GetNode<Button>("RecordButton").Text = "Record";
            GetNode<Label>("Status").Text = "";
            
        }
        else
        {
            GetNode<Button>("SaveButton").Disabled = true;
            _effect.SetRecordingActive(true);
            GetNode<Button>("RecordButton").Text = "Stop";
            GetNode<Label>("Status").Text = "Recording...";
        }
    }


    public void OnSaveButtonPressed()
    {
        string savePath = GetNode<LineEdit>("SaveButton/Filename").Text;
        _recording.SaveToWav(savePath);
        GetNode<Label>("Status").Text = string.Format("Saved WAV file to: {0}\n({1})", savePath, ProjectSettings.GlobalizePath(savePath));
    }

    public override void _Process(float delta)
    {
        base._Process(delta);
        delta_count += delta;
        GD.Print(delta_count);
        GD.Print(delta);

        if (_effect.IsRecordingActive() && delta_count >= interval)
        {
            _recording = _effect.GetRecording();
            if (_recording != null && _recording.Data.Length != 0)
            {
                var new_data = _recording.Data.Skip(recordingIndex).ToArray();
                recordingIndex = _recording.Data.Length;
                var sizeInBytes = new_data.Length;
                var sizeInFloats = sizeInBytes / sizeof(short);
                float[][] _data = new float[2][];
                for (var i = 0; i < 2; i++)
                    _data[i] = new float[sizeInFloats];
                ByteConverter.ToFloats16Bit(new_data, _data);
                GD.Print(_data[0].Length);
                var single = new DiscreteSignal(44100, _data[0]);
                var pitch = Pitch.FromAutoCorrelation(single, 0, -1, 100, 1500);
                GetNode<Label>("Pitch").Text = pitch.ToString();
            }
            delta_count = 0;
        }

    }
}


public static class EnumerableUtilities
{
    public static IEnumerable<int> RangePython(int start, int stop, int step = 1)
    {
        if (step == 0)
            throw new ArgumentException("Parameter step cannot equal zero.");

        if (start < stop && step > 0)
        {
            for (var i = start; i < stop; i += step)
            {
                yield return i;
            }
        }
        else if (start > stop && step < 0)
        {
            for (var i = start; i > stop; i += step)
            {
                yield return i;
            }
        }
    }

    public static IEnumerable<int> RangePython(int stop)
    {
        return RangePython(0, stop);
    }
}