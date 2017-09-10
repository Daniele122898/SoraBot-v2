package moe.argus.git.commands;

public class CommandKey {
    private final String[] name;
    private final boolean isDjRestricted;

    public CommandKey(String[] name, boolean isDjRestricted){
        this.name = name;
        this.isDjRestricted = isDjRestricted;
    }

    public String[] getNames(){
        return name;
    }

    public boolean isDjRestricted(){
        return isDjRestricted;
    }
}
