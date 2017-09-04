package moe.argus.git.database;


import com.zaxxer.hikari.HikariDataSource;
import moe.argus.git.Utility.Utility;
import sx.blah.discord.handle.obj.IGuild;

import java.sql.*;
import java.util.ArrayList;
import java.util.HashMap;
import java.util.List;
import java.util.Map;

public class Database {
    private static Database instance;

    private static HikariDataSource ds;

    protected Connection connection;

    private Database()
    {
        initializeHikari();
        //connection = DriverManager.getConnection(Utility.DB_CONNECTION_STRING);
    }

    private static void initializeHikari(){
        ds = new HikariDataSource();
        ds.setJdbcUrl(Utility.properties.getProperty("dbUrl"));
        ds.setUsername(Utility.properties.getProperty("dbName"));
        ds.setPassword(Utility.properties.getProperty("dbPw"));
    }

    public static Database getInstance(){
        if(instance == null){
            instance = new Database();
        }
        return instance;
    }

    public List<Map<String, Object>> query(String sql){
        try {
            PreparedStatement preparedStatement = connection.prepareStatement(sql);
            preparedStatement.execute();
            ResultSet resultSet = preparedStatement.getResultSet();
            return convertResultSet(resultSet);
        } catch (SQLException e) {
            e.printStackTrace();
        }
        return null;
    }

    public List<Map<String, Object>> get(String table, String select, String key){
        try {
            List<Map<String, Object>> result = this.query("SELECT "+select+" FROM "+table+(key == null? "": " WHERE `key` = "+key+";"));
            if(result.size()>0){
                return result;
            }else{
                return null;
            }
        }catch (Exception e){
            e.printStackTrace();
        }
        return null;
    }

    public boolean insert(String table, String colums, String values){
        try {
            this.query("INSERT INTO "+table+" ("+colums+") VALUES ("+values+")");
            return true;
        } catch (Exception e) {
            return false;
        }
    }

    private List<Map<String, Object>> convertResultSet(ResultSet resultSet) {
        if (resultSet == null) {
            return null;
        }
        try {
            ResultSetMetaData metaData = resultSet.getMetaData();
            int cols = 0;

            cols = metaData.getColumnCount();

            ArrayList list = new ArrayList();

            while (resultSet.next()) {
                HashMap row = new HashMap(cols);

                for(int i = 1; i <= cols; i++) {
                    //row[metaData.getColumnName(i)] = resultSet.getObject(i);
                    row.put(metaData.getColumnName(i), resultSet.getObject(i));
                }

                list.add(row);
            }

            resultSet.close();


            return list;
        } catch (SQLException e) {
            e.printStackTrace();
        }
        return null;
    }

    public Boolean isInDjMode(IGuild guild){
        try (Connection con = ds.getConnection()){
            String query = "SELECT `DjRole` FROM `Guilds` WHERE `GuildId` = ?";
            try(PreparedStatement preparedStatement = con.prepareStatement(query)) {
                preparedStatement.setString(1, guild.getStringID());
                preparedStatement.execute();
                ResultSet resultSet = preparedStatement.getResultSet();
                List<Map<String, Object>> results = convertResultSet(resultSet);
                Boolean isDj = (Boolean) results.get(0).get("DjRole");
                return isDj;
            }
        } catch (SQLException e) {
            e.printStackTrace();
        }
        return false;
    }


    public String getPrefix(IGuild guild){
        try (Connection con = ds.getConnection()){
            String query = "SELECT `Prefix` FROM `Guilds` WHERE `GuildId` = ?";
            try(PreparedStatement preparedStatement = con.prepareStatement(query)) {
                preparedStatement.setString(1, guild.getStringID());
                preparedStatement.execute();
                ResultSet resultSet = preparedStatement.getResultSet();
                List<Map<String, Object>> results = convertResultSet(resultSet);
                String prefix = (String) results.get(0).get("Prefix");
                return prefix;
            }
        } catch (SQLException e) {
            e.printStackTrace();
        }
        return "$";
    }


}
