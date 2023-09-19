﻿namespace Logitar.Portal.Contracts.Messages;

public record Variable
{
  public Variable() : this(string.Empty, string.Empty)
  {
  }
  public Variable(string key, string value)
  {
    Key = key;
    Value = value;
  }

  public string Key { get; set; }
  public string Value { get; set; }
}