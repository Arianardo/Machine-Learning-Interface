using System;
using System.IO;

internal class TextDocument
{

	private String name;
	private String text;
	private bool beenSaved = false;
	private Boolean savedRecently = true;

	public String Name
	{
        get
        {
			return name;
        }
        set
        {
			name = value;
        }
	}

	public String Text
	{
		get
		{
			return text;
		}
		set
		{
			text = value;
		}
	}

	public bool SavedRecently
	{
		get
		{
			return savedRecently;
		}
		set
		{
			savedRecently = value;
		}
	}

	public bool BeenSaved
	{
		get
		{
			return beenSaved;
		}
		set
		{
			beenSaved = value;
		}
	}


	public TextDocument()
	{
		this.Name = "Make a name for this file";
		this.Text = "";
	}

	public TextDocument(String fileName, String fileContent)
	{
		this.Name = fileName;
		this.Text = fileContent;
	}

	public void Save(String filename, String Content)
	{
		this.Text = Content;
		using (StreamWriter writer = new StreamWriter(filename))
		{
			writer.Write(Content);
		}
		this.SavedRecently = true;
		this.BeenSaved = true;
	}

	public void Open(String filename)
	{
		this.Name = filename;
		using (StreamReader reader = new StreamReader(filename))
		{
			this.Text = reader.ReadToEnd();
		}
		this.SavedRecently = true;
		this.BeenSaved = true;
	}
}
