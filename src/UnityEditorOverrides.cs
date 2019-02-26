namespace Apkd.Internal
{
    static class UnityEditorOverrides
    {
        static readonly StringBuilder cachedBuilderLarge = new StringBuilder(capacity: 4096);
        static readonly StringBuilder cachedBuilderSmall = new StringBuilder(capacity: 1024);
        
        static string ExtractFormattedStackTrace(StackTrace stackTrace)
        {
            try
            {
                return new EnhancedStackTrace(stackTrace).ToString();
            }
            catch
            {
                return $"Failed to post-process stacktrace:\n" + stackTrace;
            }
        }

        static string PostprocessStacktrace(string oldString, bool stripEngineInternalInformation)
        {
            if (oldString == null)
                return String.Empty;

            var output = cachedBuilderLarge.Clear();
            output.Append('\n');

            var temp = cachedBuilderSmall.Clear();
            for (int i = 0; i < oldString.Length; ++i)
            {
                char c = oldString[i];
                temp.Append(c);

                if (c == '\n' || i == oldString.Length - 1)
                {
                    (bool skip, bool exit) = PostProcessLine(temp, stripEngineInternalInformation);

                    if (exit)
                        break;

                    if (!skip)
                        for (int j = 0; j < temp.Length; ++j)
                            output.Append(temp[j]);

                    temp.Clear();
                }
            }

            return output.ToString();

            (bool skip, bool exit) PostProcessLine(StringBuilder line, bool ignoreInternal)
            {
                // Ignore empty lines
                if (line.Length == 0 || line.Length == 1 && line[0] == '\n')
                    return (skip: true, exit: false);

                // Make GameView GUI stack traces skip editor GUI part
                if (ignoreInternal && line.StartsWith("UnityEditor.EditorGUIUtility:RenderGameViewCameras"))
                    return (skip: false, exit: true);

                line.Insert(0, "│ ");

                // Unify path names to unix style
                line.Replace('\\', '/');

                return (skip: false, exit: false);
            }
        }

        static void ExtractStringFromExceptionInternal(System.Object topLevel, out string message, out string stackTrace)
        {
            try
            {
                StringBuilder temp = cachedBuilderLarge.Clear();
                Exception current = topLevel as System.Exception;
                message = current.Message != null ? current.GetType() + ": " + current.Message : current.GetType().ToString();

                while (current != null)
                {
                    if (current == topLevel)
                    {
                        new EnhancedStackTrace(current).Append(temp);
                    }
                    else
                    {
                        temp.Append($" ---> ");
                        temp.AppendLine();

                        if (!string.IsNullOrEmpty(current.Message))
                            temp.Append(current.GetType()).Append(": ").Append(current.Message).AppendLine();
                        else
                            temp.Append(current.GetType()).AppendLine();

                        new EnhancedStackTrace(current).Append(temp);

                        temp.AppendLine();
                        temp.Append("   --- End of inner exception stack trace ---");
                    }

                    current = current.InnerException;
                }
                new EnhancedStackTrace(new StackTrace(skipFrames: 1, fNeedFileInfo: true)).Append(temp);

                stackTrace = temp.ToString();
            }
            catch (Exception ex)
            {
                message = $"Unable to extract stack trace from exception: {(topLevel as System.Exception).GetType().Name}.";
                stackTrace = ex.ToString();
            }
        }
    }
}