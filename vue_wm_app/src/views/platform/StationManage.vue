<script setup>
import { ElMessage,ElMessageBox } from "element-plus";
import { ref, onMounted, computed } from 'vue';  
import { useRouter } from 'vue-router';
import { getStationIds, getStationsInfo,getQuantityByRegion,stationDeleteService,updateStation,addStation} from '@/api/platform';
import { getMerchantIds,getMerchantsInfo} from '@/api/user';
import { EditMerchantStation, assignStationToMerchant,AssignStation } from '@/api/merchant';
const router = useRouter();
const stationIds = ref([]);  //获取所有站点id
const stationsInfo = ref([]); // 存储所有站点信息 
const showStationsInfo = ref([]); // 显示站点信息列表
const searchQuery = ref(''); // 搜索框内容
const currentStation = ref(null); // 当前选中的站点
const isEdit= ref(false); // 是否编辑
const isCreate= ref(false); // 是否创建
const refForm = ref(null); // 表单
const region = ref('');
const orderCount = ref(null);
const editRules = computed(() => {  
    const rules = {  
        stationName: [  
            { required: true, message: '请输入站点名', trigger: 'blur' },  
        ],  
        stationAddress: [  
            { required: true, message: '请输入地址', trigger: 'blur' },  
        ],  
    };  
    return rules; // 返回动态生成的规则  
});  
const validateField = (field) => {  //编辑规则的应用
    refForm.value.validateField(field, (valid) => {  
        if (!valid) {  
            console.log(`验证失败: ${field}`); // 可以根据需要修改  
        }  
    });  
};  
onMounted(() => {
    getStationIds().then(res => {  // 获取所有商家id
        stationIds.value = res.data;  
        return Promise.all(stationIds.value.map(id => getStationsInfo(id))); // 并发请求所有商家信息
    }).then(responses => {  
        stationsInfo.value = responses.map(response => response.data); // 提取商家信息  
        showStationsInfo.value = stationsInfo.value; // 显示商家信息列表
    }).catch(err => {  
        ElMessage.error('获取站点id失败'); 
    }); 
});


const fetchOrderCount = async () => {
    try {
        const response = await getQuantityByRegion(region.value);
        console.log('Processed response:', response); // 增加日志输出以验证处理后的数据
        orderCount.value = response.orderCount;
        region.value = response.region; // 确保也更新了区域名称
        ElMessage.success(`区域 ${response.region} 的订单量为: ${response.orderCount}`);
    } catch (error) {
        console.error('Error fetching order count:', error);
        ElMessage.error('查询失败');
    }
};

const reassignAllMerchantStationIds = async () => {  
    try {  
        // 1. 获取所有商户 ID  
        const merchantResponse = await getMerchantIds();
        const merchantIds = merchantResponse ? merchantResponse.data : [];
        console.log(merchantIds);
        // 2. 遍历每个商户 ID  
        for (const merchantId of merchantIds) {  
            // 获取商户信息  
            const merchantsResponse = await getMerchantsInfo(merchantId);  
            const merchantInfo=merchantsResponse ? merchantsResponse.data : [];
            const merchantAddress = merchantInfo.merchantAddress; // 假设商户信息中有 Address 字段  
            // 确保地址有效  
            if (!merchantAddress) {  
                console.log(`No address found for Merchant ID ${merchantId}`);  
                continue; // 跳过没有地址的商户  
            }  
            // 3. 获取新 Station ID  
            const newStationId = await assignStationToMerchant(merchantAddress);  
            if (newStationId) {  
                // 4. 更新商户的 Station ID  
                const updateData = {  
                    MerchantId: merchantId,  
                    StationId: newStationId  
                };  
                await AssignStation(updateData);  
                await EditMerchantStation(updateData);  
                console.log(`Merchant ID ${merchantId} reassigned to Station ID ${newStationId}`);  
            } else {  
                console.log(`No station found for Merchant ID ${merchantId} with address ${merchantAddress}`);  
            }  
        }  
    } catch (error) {  
        console.error('Error reassigning merchant station IDs:', error);  
    }  
};  
const gobackHome = () => {
    router.push('/platform-home');

}
const handleSearch = () => {   //字符串匹配搜索商家
    const query = searchQuery.value.trim();  
    if (query) {  
        showStationsInfo.value = stationsInfo.value.filter(station =>  
            station.stationName.includes(query) ||  
            station.stationAddress.includes(query)  
        );  
    } else {  
        showStationsInfo.value = stationsInfo.value;  
    }
};  
const deleteStation = async (stationId) => {  
    ElMessageBox.confirm('确认要删除该站点吗?', '提示', {  
        confirmButtonText: '确定',  
        cancelButtonText: '取消',  
        type: 'warning'  
    }).then(() => {  
        stationDeleteService(stationId).then(res => {  
            ElMessage.success('删除成功');  
            reassignAllMerchantStationIds();
            showStationsInfo.value = showStationsInfo.value.filter(station => station.stationId!== stationId); 
        }).catch(err => {  
            ElMessage.error('删除失败');  
        });  
    }).catch(() => {  
        console.log('取消删除');  
    });  
};  
const enterEdit = (station) => {  
    currentStation.value = station;  
    isEdit.value = true;  
};  
const enterCreate = () => {  
    currentStation.value = {  
        stationId: 0,  
        stationName: '',  
        stationAddress: '',  
    };  
    isCreate.value = true;  
};  
const cancelEdit = () => {  
    currentStation.value = null;  
    isEdit.value = false;  
};  
const cancelCreate = () => {  
    currentStation.value = null;  
    isCreate.value = false;  
};  
const submitEdit = async() => {  
    const isValid = await refForm.value.validate(); 
    if (!isValid) return; 
    updateStation(currentStation.value).then(res => {  
        ElMessage.success('修改成功');  
        reassignAllMerchantStationIds();
        isEdit.value = false;  
        showStationsInfo.value = showStationsInfo.value.map(station => {  
            if (station.stationId === currentStation.value.stationId) {  
                return currentStation.value;  
            }  
            return station;  
        });  
        currentStation.value = null;  
    }).catch(err => {  
        ElMessage.error('站点未修改');  
    }); 
};
const submitCreate = async() => {  
    const isValid = await refForm.value.validate(); 
    if (!isValid) return; 
    const data=ref({
        StationId:0,
        StationName:currentStation.value.stationName,
        StationAddress:currentStation.value.stationAddress,
    });
    console.log(data.value);
    addStation(data.value).then(res => {  
        ElMessage.success('创建成功');  
        reassignAllMerchantStationIds();
        currentStation.value.stationId = res.data;  //更新站点id
        showStationsInfo.value.push(currentStation.value);  
        isCreate.value = false;  
        currentStation.value = null;  
    }).catch(err => {  
        ElMessage.error('创建失败');  
    }); 
};
</script>

<template>
    <div v-if="!isEdit&&!isCreate" class="box">
        <div class="head">站点列表 </div>
        <div class="top">
            <input type="text" v-model="searchQuery" placeholder="搜索站点名或地址" v-on:keyup.enter="handleSearch()" class="inputtext"/> 
            <button @click="handleSearch()" class="search">搜索</button>
            <button @click="enterCreate()" class="release">创建</button>
        </div>
        <el-table :data="showStationsInfo" class="table" border>
        <el-table-column prop="stationName" label="站点名称" width=250>
            <template #default="{ row }">
                {{ row.stationName }}
            </template>
        </el-table-column>
        <el-table-column prop="stationAddress" label="站点地址" width=500>
            <template #default="{ row }">
                {{ row.stationAddress }}
            </template>
        </el-table-column>
        <el-table-column label="操作" width=250>
            <template #default="{ row }">
                <el-button type="primary" size="small" @click="enterEdit(row)">编辑</el-button>
                <el-button type="danger" size="small" @click="deleteStation(row.stationId)">删除</el-button>
            </template>
        </el-table-column>
    </el-table>
        <button @click="gobackHome" class="return">返回</button>
    </div>
    
    <div v-if="isEdit" class="box2">
        <div class="head3">编辑站点</div> 
        <el-form :rules="editRules" ref="refForm" :model="currentStation">
            <el-form-item  prop="stationName">
                <div class="info">*站点名</div>
                <input v-model="currentStation.stationName" placeholder="站点名" @blur="validateField('stationName')" class="inputtext"/>
            </el-form-item>
            <el-form-item  prop="stationAddress">
                <div class="info">*站点地址(建议详细到门牌号)</div>
                <input v-model="currentStation.stationAddress" placeholder="地址" @blur="validateField('stationAddress')" class="inputtext"/>
            </el-form-item>
        </el-form>  
        <div class="bottom">
        <button @click="submitEdit" class="choose2">提交</button>  
        <button @click="cancelEdit" class="choose2">取消</button>
    </div>
    </div>
    <div v-if="isCreate" class="box2">
        <div class="head3">创建站点</div> 
        <el-form :rules="editRules" ref="refForm" :model="currentStation">
                <el-form-item prop="stationName" >
                    <div class="info">*站点名</div>
                    <input v-model="currentStation.stationName" placeholder="站点名" @blur="validateField('stationName')" class="inputtext"/>
                </el-form-item>
                <el-form-item  prop="stationAddress" >
                    <div class="info">*站点地址(建议详细到门牌号)</div>
                    <input v-model="currentStation.stationAddress" placeholder="地址" @blur="validateField('stationAddress')" class="inputtext"/>
                </el-form-item>
        </el-form>  
        <div class="bottom">
        <button @click="submitCreate" class="choose2">提交</button>  
        <button @click="cancelCreate" class="choose2">取消</button>
        </div>
    </div>
</template>

<style scoped lang="scss">
body {
    background-color: #e59090; /* 选择你喜欢的颜色 */
}
.info{
    display:flex;
    width:30%;
    font-size: 18px;
    margin-left: 5%;

}

.inputtext{
    height: 30px;
    width: 450px;
    right:5%;
    font-size: 2.8vmin;
    border-radius: 9px;
    margin-right: 2%;
}
.bottom{
    display:flex;
    justify-content: center; /* 水平居中 */
    align-items: center; /* 垂直居中 */
    margin-top: 5%;
}
.top{
    display:flex;
    justify-content: center; /* 水平居中 */
    align-items: center; /* 垂直居中 */
}
.box{
    padding: 20px;
    background-color: #7ac2ee;
    border: 2px solid #000000;
    border-radius: 20px;
    margin-right: 30px;
    box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);

    font-size: 3vmin; /* 字体大小 */
    position: fixed; /* 固定定位 */
    top: 60px; /* 贴近顶部 */
    left: 50%; /* 水平居中 */
    transform: translateX(-50%); /* 修正水平居中 */
    width: 70%;
    height: 80%;
}
.box2{
    padding: 20px;
    background-color: #7ac2ee;
    border: 2px solid #000000;
    border-radius: 20px;
    margin-right: 30px;
    box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);

    font-size: 3vmin; /* 字体大小 */
    position: fixed; /* 固定定位 */
    top: 60px; /* 贴近顶部 */
    left: 50%; /* 水平居中 */
    transform: translateX(-50%); /* 修正水平居中 */
    width: 60%;
    height: 70%;
}
.return{
    position: absolute;
    right: 10px;
    bottom: 10px;
    background-color: #FFC0CB;
    font-size: 2.4vmin; 
}
.return:hover{
    background-color: #f7ced5;
}
.head{
    display:flex;
    justify-content: center; /* 水平居中 */
    align-items: center; /* 垂直居中 */
    left:50%;
    font-size: 4.5vmin; /* 字体大小 */
    color:#000000;

}
.head3{
    display:flex;
    justify-content: center; /* 水平居中 */
    align-items: center; /* 垂直居中 */
    left:50%;
    font-size: 4.5vmin; /* 字体大小 */
    color:#000000;
    margin-bottom: 5%;
}
.choose{
    padding: 5px 8px;         /* 按钮内边距 */
    margin-right: 8px;         /* 按钮右边距 */
    background-color: #f68396;
    font-size: 2.5vmin; /* 字体大小 */
}
.choose:hover{
    background-color: #FFC0CB;
}
.choose2{
    padding: 5px 8px;         /* 按钮内边距 */
    margin-right: 8px;         /* 按钮右边距 */
    background-color: #f68396;
    font-size: 3vmin; /* 字体大小 */
}
.choose2:hover{
    background-color: #FFC0CB;
}
.head2{
    margin-top:4%;
    margin-bottom: 3%;
    display:flex;
    justify-content: center; /* 水平居中 */
    align-items: center; /* 垂直居中 */
    color:#f56a81;
}
.input{
    display:flex;
    justify-content: center; /* 水平居中 */
    align-items: center; /* 垂直居中 */
    margin-left:30%;
    margin-right:30%;
    width:40%;
    font-size: 3vmin;
}
.output{
    display:flex;
    justify-content: center; /* 水平居中 */
    align-items: center; /* 垂直居中 */
    margin-top:1%;
   // width:100%;
    font-size: 3vmin;
    color:#583def;
}
.search{
    padding: 5px 8px;         /* 按钮内边距 */
    margin-right: 8px;         /* 按钮右边距 */
    background-color: #FFC0CB;
    font-size: 2.2vmin; /* 字体大小 */
}
.release{
    padding: 5px 8px;         /* 按钮内边距 */
    margin-right: 8px;         /* 按钮右边距 */
    background-color: #FFC0CB;
    font-size: 2.2vmin; /* 字体大小 */
}
.table{
    margin-top:10px;
    margin-left:5%;
    height:77%;
    width:90%;
    border-radius: 20px;
    border: 2px solid #01042a;
    table-layout: auto;
}
</style>