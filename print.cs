﻿using System;
using System.Collections.Generic;
using System.Text;

public class print
{
    public static void log(params object[] p)
    {
        if (Crunch.Testing.Debug || Crunch.Testing.ShowWork)
        {
            string s = p[0].ToString();
            for (int i = 1; i < p.Length; i++)
                s += ", " + p[i];
            System.Diagnostics.Debug.WriteLine(s);
        }
    }
}