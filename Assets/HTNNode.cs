using System.Collections.Generic;

public class HTNNode
{
    List<HTNNode> children = new List<HTNNode>();
    List<HTNNode> subtasks = new List<HTNNode>();
    string type = "";
    string name = "";

    public HTNNode(string type, List<HTNNode> children, string name)
    {
        this.name = name;
        this.type = type;
        this.children = children;
    }

    public HTNNode(string type, string name)
    {
        this.name = name;
        this.type = type;
    }

    public HTNNode()
    {
        this.subtasks = new List<HTNNode>();
        this.children = new List<HTNNode>();

        this.type = "";
    }


    public void setChildren(List<HTNNode> children)
    {
        this.children = children;

    }

    public void setSubtasks(List<HTNNode> subtasks)
    {
        this.subtasks = subtasks;

    }

    public List<HTNNode> getSubtasks()
    {
        return this.subtasks;

    }

    public List<HTNNode> getChildren()
    {
        return this.children;
    }

    public void setType(string type)
    {

        this.type = type;
    }

    public string getType()
    {
        return this.type;
    }
    public void setName(string name)
    {

        this.name = name;
    }

    public string getName()
    {
        return this.name;
    }

}
