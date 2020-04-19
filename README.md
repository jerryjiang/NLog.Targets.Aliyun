# NLog Target for Aliyun Log Service

## Steps
1. Create a Log Service project and logstore
2. Enable RAM access control and create an AccessKey
3. Endpoint, project, accessKeyId and accessKey does not support layout rendering
4. When use layout rendering for logStore, make sure you created the specified logstore in SLS portal
5. In the example below, the log will send to development, staging or production logstore depends on the environment, but logs without the context will be dropped
6. Source will default to Local IP address if not set
7. Topic will default to logger name if not set

## Supported Configuration

- **name** - _Specify a name, but make sure you use the same name in logger rules section_
- **endpoint** - _You can find it in the home screnn of your log service project. Use external endpoint if your application is not deployed within Aliyun_
- **project** - _The log service project name you specified_
- **accessKeyId** - _The RAM access key id_
- **accessKey** - _The RAM access key_
- **logStore** - _The log service log store anem you specified_
- **source** - _Leave this as empty or remove this attribute will send the IP address the application are running_
- **topic** - _Leave this empty or remove this attribute will send the logger name_
- **layout** - _The message layout_

## Configuration Example

- Add NLog.Targets.Aliyun Extension

        <add assembly="NLog.Targets.Aliyun"/>

- Add Aliyun Target

        <target xsi:type="Aliyun" name="aliyun"
                endpoint="<your sls endpoint>"
                project="<your project name>"
                accessKeyId="<your accessKeyId>"
                accessKey="<your accessKey>"
                logStore="${lowercase:${aspnet-environment}}"
                source="<anything you want>"
                topic="<anything you want>"
                layout="${longdate} | ${message} | ${exception:format=ToString,StackTrace}"
        />

- Add Logger Rules

        <logger name="*" minlevel="Debug" writeTo="aliyun" />

See [full example](demo/NLog.config) in the demo project.