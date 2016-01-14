﻿using System.Collections.Generic;
using System.Reflection;
using Windows.UI.Xaml;

namespace Geeky.UI.Helpers
{
    public static class VisualTreeHelper
    {
        public static void FindChildren<T>(List<T> results, DependencyObject startNode) where T : DependencyObject
        {
            int count = Windows.UI.Xaml.Media.VisualTreeHelper.GetChildrenCount(startNode);
            for (int i = 0; i < count; i++)
            {
                DependencyObject current = Windows.UI.Xaml.Media.VisualTreeHelper.GetChild(startNode, i);
                if ((current.GetType()) == typeof(T) || (current.GetType().GetTypeInfo().IsSubclassOf(typeof(T))))
                {
                    T asType = (T)current;
                    results.Add(asType);
                }
                FindChildren<T>(results, current);
            }
        }
    }
}
