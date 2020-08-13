using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ActionData
{
  public Data data;
  public string[] logs;
}

[System.Serializable]
public class Data
{
  public float left;
  public float right;
}

[System.Serializable]
public class Logs
{
  public string[] stdout;
}
