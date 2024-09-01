<script setup>
import { useStore } from "vuex";
import { ElMessage,ElMessageBox } from "element-plus";
import { ref, onMounted, computed } from 'vue';  
import { useRouter } from 'vue-router';
import { getStationIds, getStationsInfo,stationDeleteService,updateStation,addStation} from '@/api/platform';
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
    <div v-if="!isEdit&&!isCreate">
        <div>
            片区管理
            <button @click="gobackHome">返回</button>
        </div>
        <h2>站点列表</h2>  
        <div>
            <input type="text" v-model="searchQuery" placeholder="搜索站点名或地址" v-on:keyup.enter="handleSearch()"/> 
            <button @click="handleSearch()">搜索</button>
            <button @click="enterCreate()">创建</button>
        </div>
        <ul>  
            <li v-for="station in showStationsInfo" :key="station.stationId">  
                <span>{{ station.stationName }}</span> 
                <span>&nbsp;&nbsp;{{ station.stationAddress }}</span>
                <span>&nbsp;<button @click="enterEdit(station)">编辑</button></span>
                <span>&nbsp;<button @click="deleteStation(station.stationId)">删除</button></span>
            </li>  
        </ul> 
    </div>
    <div v-if="isEdit">
        <h2>编辑站点</h2> 
        <el-form :rules="editRules" ref="refForm" :model="currentStation">
            <el-form-item label="站点名" prop="stationName"><input v-model="currentStation.stationName" placeholder="站点名" @blur="validateField('stationName')"/></el-form-item>
            <el-form-item label="站点地址(建议详细到门牌号)" prop="stationAddress"><input v-model="currentStation.stationAddress" placeholder="地址" @blur="validateField('stationAddress')"/></el-form-item>
        </el-form>  
        <button @click="submitEdit">提交</button>  
        <button @click="cancelEdit">取消</button>
    </div>
    <div v-if="isCreate">
        <h2>创建站点</h2> 
        <el-form :rules="editRules" ref="refForm" :model="currentStation">
            <el-form-item label="站点名" prop="stationName"><input v-model="currentStation.stationName" placeholder="站点名" @blur="validateField('stationName')"/></el-form-item>
            <el-form-item label="站点地址(建议详细到门牌号)" prop="stationAddress"><input v-model="currentStation.stationAddress" placeholder="地址" @blur="validateField('stationAddress')"/></el-form-item>
        </el-form>  
        <button @click="submitCreate">提交</button>  
        <button @click="cancelCreate">取消</button>
    </div>
</template>