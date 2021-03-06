﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GenerateBaseConfiguration.KV
{
    /// <summary>
    /// This is largely drawn from https://github.com/AntonAderum/KVLib, and then
    /// modified to be re-entrant, thread-safe, and capable of handling more stuff.
    /// </summary>
    static class KVParser
    {
        /// <summary>
        /// An enum used for parse state tracking.
        /// </summary>
        enum parseEnum { lookingForKey, lookingForValue };

        /// <summary>
        /// Grab all of the keyvalues from a string.
        /// </summary>
        /// <param name="contents">The string containing keyvalues</param>
        /// <param name="allowunnamedkeys">Whether or not to allow unnamed blocks (used in bsp entity lump)</param>
        /// <returns>An array containing all root-level KeyValues in the string</returns>
        public static KeyValue[] ParseAllKeyValues(string contents, bool allowunnamedkeys = false)
        {
            parseEnum parseState = parseEnum.lookingForKey;
            KeyValue basekv = new KeyValue("base"); // file contents are interpreted as children of this keyvalue
            KeyValue curparent = basekv;
            for (int i = 0; i < contents.Length; i++)
            {
                // go until next symbol
                if (contents[i] == ' ' || contents[i] == '\t' || contents[i] == '\n' || contents[i] == '\r')
                    continue;
                switch (parseState)
                {
                    case parseEnum.lookingForKey:
                        if (contents[i] == '{')
                        {
                            if (!allowunnamedkeys)
                            {
                                throw new KeyValueParsingException("Hit unnamed key while parsing without unnamed keys enabled.", null);
                            }
                            // This is a special case - some kv files, in particular bsp entity lumps, have unkeyed kvs
                            KeyValue cur = new KeyValue("UNNAMED");
                            curparent.AddChild(cur);
                            curparent = cur;
                            parseState = parseEnum.lookingForValue;
                        }
                        else if (contents[i] == '"' || contents[i] == '\'')
                        {
                            //quoted key
                            int j = i + 1;
                            if(j >= contents.Length)
                            {
                                throw new KeyValueParsingException("Couldn't find terminating '" + contents[i].ToString() + "' for key started at position " + i.ToString(), null);
                            }
                            while (contents[j] != contents[i])
                            {
                                // handle escaped quotes
                                if (contents[j] == '\\')
                                {
                                    j++;
                                }
                                j++;
                                if (j >= contents.Length)
                                {
                                    throw new KeyValueParsingException("Couldn't find terminating '" + contents[i].ToString() + "' for key started at position " + i.ToString(), null);
                                }
                            }
                            //ok, now contents[i] and contents[j] are the same character, on either end of the key
                            KeyValue cur = new KeyValue(contents.Substring(i + 1, j - (i + 1)));
                            curparent.AddChild(cur);
                            curparent = cur;
                            parseState = parseEnum.lookingForValue;
                            i = j;
                        }
                        else if (Char.IsLetter(contents[i]))
                        {
                            //un-quoted key
                            int j = i;
                            while (contents[j] != ' ' && contents[j] != '\t' && contents[j] != '\n' && contents[j] != '\r')
                            {
                                j++;
                                if (j > contents.Length)
                                {
                                    throw new KeyValueParsingException("Couldn't find end of key started at position " + i.ToString(), null);
                                }
                            }
                            KeyValue cur = new KeyValue(contents.Substring(i, j - i));
                            curparent.AddChild(cur);
                            curparent = cur;
                            parseState = parseEnum.lookingForValue;
                            i = j;
                        }
                        else if (contents[i] == '}')
                        {
                            //drop one level
                            curparent = curparent.Parent;
                        }
                        else if (contents[i] == '/')
                        {
                            if (i + 1 < contents.Length && contents[i + 1] == '/')
                            {
                                // we're in a comment! throw stuff away until the next \n
                                while (i < contents.Length && contents[i] != '\n')
                                {
                                    i++;
                                }
                            }
                        }
                        else
                        {
                            throw new KeyValueParsingException("Unexpected '" + contents[i].ToString() + "' at position " + i.ToString(), null);
                        }
                        break;
                    case parseEnum.lookingForValue:
                        if (contents[i] == '{')
                        {
                            // it's a list of children
                            // thankfully, we don't actually need to handle this!
                            parseState = parseEnum.lookingForKey;
                        }
                        else if (contents[i] == '"' || contents[i] == '\'')
                        {
                            //quoted value
                            int j = i + 1;
                            while (contents[j] != contents[i])
                            {
                                // handle escaped quotes
                                if (contents[j] == '\\')
                                {
                                    j++;
                                }
                                j++;
                                if (j > contents.Length)
                                {
                                    throw new KeyValueParsingException("Couldn't find terminating '" + contents[i].ToString() + "' for key started at position " + i.ToString(), null);
                                }
                            }
                            //ok, now contents[i] and contents[j] are the same character, on either end of the value
                            curparent.Set(contents.Substring(i + 1, j - (i + 1)));
                            curparent = curparent.Parent;
                            parseState = parseEnum.lookingForKey;
                            i = j;
                        }
                        else if (contents[i] == '/')
                        {
                            if (i + 1 < contents.Length && contents[i + 1] == '/')
                            {
                                // we're in a comment! throw stuff away until the next \n
                                while (i < contents.Length && contents[i] != '\n')
                                {
                                    i++;
                                }
                            }
                        }
                        else if (!Char.IsWhiteSpace(contents[i]))
                        {
                            int j = i;
                            while (contents[j] != ' ' && contents[j] != '\t' && contents[j] != '\n' && contents[j] != '\r')
                            {
                                j++;
                                if (j > contents.Length)
                                {
                                    // a value ending the file counts as ending the value
                                    break;
                                }
                            }
                            curparent.Set(contents.Substring(i, j - i));
                            curparent = curparent.Parent;
                            parseState = parseEnum.lookingForKey;
                            i = j;
                        }
                        else
                        {
                            throw new KeyValueParsingException("Unexpected '" + contents[i].ToString() + "' at position " + i.ToString(), null);
                        }
                        break;
                }
            }
            // At the end of the file, we should be looking for another key
            if (parseState != parseEnum.lookingForKey)
            {
                throw new KeyNotFoundException("File ended while looking for value", null);
            }
            // At the end of the file, all block values should be closed
            if (curparent != basekv)
            {
                throw new KeyNotFoundException("Unterminated child blocks", null);
            }
            KeyValue[] ret = basekv.Children.ToArray<KeyValue>();
            basekv.clearChildParents();
            return ret;
        }
        /// <summary>
        /// Parse a single KeyValue from a string.
        /// </summary>
        /// <param name="contents">The string representation of the KeyValue object</param>
        /// <returns>The parsed KeyValue</returns>
        public static KeyValue ParseKeyValue(string contents)
        {
            KeyValue[] all = ParseAllKeyValues(contents);
            if (all.Length == 0)
                return null;
            return all[0];
        }
        /// <summary>
        /// Parse a single KeyValue from a file.
        /// </summary>
        /// <param name="path">The path to the file.</param>
        /// <returns>The parsed KeyValue</returns>
        public static KeyValue ParseKeyValueFile(string path)
        {
            string contents = System.IO.File.ReadAllText("SampleKeyValues/" + path, Encoding.BigEndianUnicode);
            return ParseKeyValue(contents);
        }
        /// <summary>
        /// An exception thrown when parsing a KV file.
        /// </summary>
        public class KeyValueParsingException : Exception
        {
            /// <summary>
            /// Construct a new KeyValueParsingException
            /// </summary>
            /// <param name="message">The message to throw.</param>
            /// <param name="inner">The internal exception that caused the KVPE</param>
            public KeyValueParsingException(string message, Exception inner)
                : base(message, inner)
            {

            }
        }
    }
}
