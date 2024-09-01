<script setup>
import { useStore } from "vuex";
import { ElMessage,ElMessageBox } from "element-plus";
import { ref, onMounted, watch } from 'vue';  
import { useRouter } from 'vue-router';
import {getStationIds,separateRiders,getStationsInfo,assignRiderStation,deleteRiderStation,editRiderStation} from '@/api/platform'
import { riderInfo,stIdSearch, getDeliveredOrdersCountandAverageRating } from '@/api/rider'
const router = useRouter();
const unAssignedRiderIds = ref([]);  //未分配骑手id
const assignedRiderIds = ref([]);  //已分配骑手id
const RiderIds= ref([]);
const ridersInfo = ref([]);  //所有骑手信息
const asRidersInfo = ref([]);  //已分配骑手信息
const unRidersInfo= ref([]);  //未分配骑手信息
const showUnRidersInfo = ref([]);  //显示的未分配骑手信息
const showAsRidersInfo = ref([]);  //显示的已分配骑手信息
const isUnAssigned = ref(true);  //默认显示未分配站点的骑手
const currentRider = ref({});  //当前选择的骑手id
const searchQuery = ref('');  //搜索框内容
const isAssigning = ref(false);  //是否正在分配站点
const searchStation = ref('');  //搜索站点内容
const stationIds = ref([]);  //获取所有站点id
const stationsInfo = ref([]); // 存储所有站点信息 
const showStationsInfo = ref([]); // 显示站点信息列表
const isEditAssigning = ref(false);  //是否正在编辑分配站点

const sortField = ref('deliveredOrdersCount'); // 默认排序字段
const sortOrder = ref('desc'); // 默认排序方向，'asc'表示升序，'desc'表示降序

 
onMounted(async () => {
    try {  
        const res = await separateRiders();  // 获取所有骑手的 ID  
        unAssignedRiderIds.value = res.data1;  
        assignedRiderIds.value = res.data2;  
        RiderIds.value = [...unAssignedRiderIds.value, ...assignedRiderIds.value];   
        
        // 获取所有骑手信息  
        //const responses = await Promise.all(RiderIds.value.map(id => riderInfo(id)));   

        // 获取所有骑手信息和他们的平均评分
        const responses = await Promise.all(RiderIds.value.map(async id => {
            const riderInfoResponse = await riderInfo(id); // 获取骑手信息
            const averageRatingandOrdersCountResponse = await getDeliveredOrdersCountandAverageRating(id); // 获取骑手的平均评分
            //console.info(averageRatingandOrdersCountResponse);
            return {
                ...riderInfoResponse.data,
                averageRating: averageRatingandOrdersCountResponse.averageRating, // 添加平均评分到骑手信息中
                deliveredOrdersCount:averageRatingandOrdersCountResponse.deliveredOrdersCount, //添加送达订单量
            };
        }));

        ridersInfo.value = responses; // 更新ridersInfo以包含骑手信息和平均评分

        //ridersInfo.value = responses.map(response => response.data); 
        //console.info(ridersInfo.value);  
        
        // 筛选出已分配和未分配的骑手信息  
        asRidersInfo.value = ridersInfo.value.filter(rider => assignedRiderIds.value.includes(rider.riderId));  
        unRidersInfo.value = ridersInfo.value.filter(rider => unAssignedRiderIds.value.includes(rider.riderId));  

        // 对已分配的骑手获取站点信息  
        await Promise.all(asRidersInfo.value.map(async (rider) => {  
            if (rider.riderId) {  
                const stationId = await stIdSearch(rider.riderId);  
                const stationData = await getStationsInfo(stationId.data);  
                rider.stationId = stationData.data.stationId || ''; // 站点Id
                rider.stationName = stationData.data.stationName || ''; // 站点名
                rider.stationAddress = stationData.data.stationAddress || ''; // 站点地址
            }  
        }));  
        // 更新显示列表  
        showUnRidersInfo.value = unRidersInfo.value;  
        showAsRidersInfo.value = asRidersInfo.value;  

        // 调用排序函数
        sortRiders();

    } catch (err) {  
        ElMessage.error('获取骑手信息失败');   
    }  
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

// 给骑手排序
const sortRiders = () => {
    const sortedUnRidersList = [...unRidersInfo.value];
    const sortedAsRidersList = [...asRidersInfo.value];

    const compare = (a, b) => {
        const aValue = a[sortField.value] !== undefined ? a[sortField.value] : (sortField.value === 'averageRating' ? 0 : 0);
        const bValue = b[sortField.value] !== undefined ? b[sortField.value] : (sortField.value === 'averageRating' ? 0 : 0);

        //const aValue = a[sortField.value];
        //const bValue = b[sortField.value];

        if (sortOrder.value === 'asc') {
            return aValue < bValue ? -1 : aValue > bValue ? 1 : 0;
        } else {
            return aValue > bValue ? -1 : aValue < bValue ? 1 : 0;
        }
    };

    sortedUnRidersList.sort(compare);
    sortedAsRidersList.sort(compare);

    showUnRidersInfo.value = sortedUnRidersList;
    showAsRidersInfo.value = sortedAsRidersList;
};

const getStationId= (riderId) => {  //获取站点id
    stIdSearch(riderId).then(res => {  
        console.log("站点id:", res.data);  
        return res.data;
    }).catch(err => {  
        ElMessage.error('获取站点id失败'); 
    }); 
}
const gobackHome = () => {
    router.push('/platform-home');
}
const handleUnSearch = () => {
    showUnRidersInfo.value = unRidersInfo.value.filter(rider => String(rider.riderId).includes(searchQuery.value) || String(rider.riderName).includes(searchQuery.value));
}
const handleAsSearch = () => {
    showAsRidersInfo.value = asRidersInfo.value.filter(rider => String(rider.riderId).includes(searchQuery.value) || String(rider.riderName).includes(searchQuery.value) || String(rider.stationName).includes(searchQuery.value));
}
const enterAssign = (rider) => {
    currentRider.value = rider;
    isAssigning.value = true;
    isEditAssigning.value = false;
}
const handleStationSearch = () => {
    showStationsInfo.value = stationsInfo.value.filter(station => String(station.stationName).includes(searchStation.value) || String(station.stationAddress).includes(searchStation.value));
}
const handleChoose = (stationId) => {
    ElMessageBox.confirm('确认要分配给该站点吗?', '提示', {  
        confirmButtonText: '确定',  
        cancelButtonText: '取消',  
        type: 'warning'  
    }).then(() => {  
        const data =ref({
            RiderId: currentRider.value.riderId,
            StationId: stationId
        });
        assignRiderStation(data.value).then(res => {  
            ElMessage.success('分配成功');  
            showUnRidersInfo.value = showUnRidersInfo.value.filter(rider => rider.riderId !== currentRider.value.riderId);  
            getStationsInfo(stationId).then(res => {  
                const toAddAsRiderInfo=ref({
                    riderId: currentRider.value.riderId,
                    riderName: currentRider.value.riderName,
                    phoneNumber: currentRider.value.phoneNumber,
                    stationId: stationId,
                    stationName: res.data.stationName,
                    stationAddress: res.data.stationAddress,
                })
                // 更新已分配骑手列表  
                showAsRidersInfo.value = [...showAsRidersInfo.value, toAddAsRiderInfo.value];  
                currentRider.value = {};  
                isAssigning.value = false;  
            }).catch(err => {  
                ElMessage.error('获取站点信息失败');  
            });  
        }).catch(err => {  
            ElMessage.error('分配失败');  
        });  
    }).catch(() => {  
        console.log('取消分配');  
    });  
}
const handleEditChoose = (stationId) => {
    ElMessageBox.confirm('确认要更换至该站点吗?', '提示', {  
        confirmButtonText: '确定',  
        cancelButtonText: '取消',  
    }).then(() => {  
        const data =ref({
            RiderId: currentRider.value.riderId,
            StationId: stationId
        });
        editRiderStation(data.value).then(res => {  
            ElMessage.success('更改成功');  
            showAsRidersInfo.value = showAsRidersInfo.value.map(rider => {  
                if (rider.riderId === currentRider.value.riderId) {  
                    rider.stationId = stationId;  
                    rider.stationName = stationsInfo.value.find(station => station.stationId === stationId).stationName;  
                    rider.stationAddress = stationsInfo.value.find(station => station.stationId === stationId).stationAddress;  
                }  
                return rider;  
            });  
            currentRider.value = {};  
            isEditAssigning.value = false;  
        }).catch(err => {  
            ElMessage.error('更改失败');  
        });  
    }).catch(() => {  
        console.log('取消更改');  
    });  
}
const leaveAssign = () => {
    currentRider.value = {};
    isAssigning.value = false;
    isEditAssigning.value = false;
}
const deleteAssign = (rider) => {
    ElMessageBox.confirm('确认要删除该骑手的站点分配吗?', '提示', {  
        confirmButtonText: '确定',  
        cancelButtonText: '取消',  
        type: 'warning'  
    }).then(() => {  
        const riderId=ref(rider.riderId);
        deleteRiderStation(riderId.value).then(res => {  
            ElMessage.success('删除成功');  
            showAsRidersInfo.value = showAsRidersInfo.value.filter(r => r.riderId !== rider.riderId);  
            showUnRidersInfo.value = [...showUnRidersInfo.value, rider];  
        }).catch(err => {  
            ElMessage.error('删除失败');  
        });  
    }).catch(() => {  
        console.log('取消删除');  
    });  
}
const enterEditAssign = (rider) => {
    currentRider.value = rider;
    isEditAssigning.value = true;
    isAssigning.value = false;
}
const leaveEditAssign = () => {
    currentRider.value = {};
    isEditAssigning.value = false;
    isAssigning.value = false;
}
</script>

<template>
    <div v-if="!isAssigning&&!isEditAssigning">
        <div>
            骑手管理
            <button @click="gobackHome">返回</button>
        </div>
        <label @click="isUnAssigned = true">未分配站点</label>&nbsp;&nbsp;
        <label @click="isUnAssigned = false">已分配站点</label>
        <div>
            <label>排序字段:</label>
            <select v-model="sortField" @change="sortRiders">
                <option value="deliveredOrdersCount">送达订单量</option>
                <option value="averageRating">评价平均分</option>
            </select>
            &nbsp;&nbsp;
            <label>排序方式:</label>
            <select v-model="sortOrder" @change="sortRiders">
                <option value="asc">升序</option>
                <option value="desc">降序</option>
            </select>
        </div>
        <div v-if="isUnAssigned">
            <div>
                <input type="text" v-model="searchQuery" placeholder="搜索Id或姓名" v-on:keyup.enter="handleUnSearch()"/> 
                <button @click="handleUnSearch()">搜索</button>
            </div>
            
            <ul>  
                <li v-for="rider in showUnRidersInfo" :key="rider.riderId">  
                    <span>Id: {{ rider.riderId }}</span>
                    <span>&nbsp;&nbsp;{{ rider.riderName }}</span> 
                    <span>&nbsp;&nbsp;{{ rider.phoneNumber }}</span>
                    <!--显示接单数与评分-->
                    <span>&nbsp;&nbsp;送单量：{{ rider.deliveredOrdersCount }}</span>
                    <span>&nbsp;&nbsp;评分：{{ rider.averageRating }}</span>
                    <button @click="enterAssign(rider)">分配站点</button>
                </li>  
            </ul>  
        </div>
        <div v-if="!isUnAssigned">
            <div>
                <input type="text" v-model="searchQuery" placeholder="搜索Id、姓名或站点名" v-on:keyup.enter="handleAsSearch()"/> 
                <button @click="handleAsSearch()">搜索</button>
            </div>
            
            <ul>  
                <li v-for="rider in showAsRidersInfo" :key="rider.riderId">  
                    <span>Id: {{ rider.riderId }}</span>
                    <span>&nbsp;&nbsp;{{ rider.riderName }}</span> 
                    <span>&nbsp;&nbsp;{{ rider.phoneNumber }}</span>
                    <span>&nbsp;&nbsp;{{ rider.stationName}}</span>
                    <span>&nbsp;&nbsp;送单量：{{ rider.deliveredOrdersCount }}</span>
                    <span>&nbsp;&nbsp;评分：{{ rider.averageRating }}</span>
                    <button @click="enterEditAssign(rider)">更改分配</button>
                    <button @click="deleteAssign(rider)">删除分配</button>
                </li>  
            </ul>  
        </div>
    </div>
    <div v-if="isAssigning">
        <div>请为骑手{{ currentRider.riderName }}分配站点：</div>
        <div>
            <input type="text" v-model="searchStation" placeholder="搜索站点名或地址" v-on:keyup.enter="handleStationSearch()"/> 
            <button @click="handleStationSearch()">搜索</button>
            <span>&nbsp;<button @click="leaveAssign()">取消</button></span>
        </div>
        <ul>  
            <li v-for="station in showStationsInfo" :key="station.stationId">  
                <span>{{ station.stationName }}</span> 
                <span>&nbsp;&nbsp;{{ station.stationAddress }}</span>
                <span>&nbsp;<button @click="handleChoose(station.stationId)">选择</button></span>
            </li>  
        </ul> 
    </div>
    <div v-if="isEditAssigning">
        <div>请为骑手{{ currentRider.riderName }}更换站点：</div>
        <div>
            <input type="text" v-model="searchStation" placeholder="搜索站点名或地址" v-on:keyup.enter="handleStationSearch()"/> 
            <button @click="handleStationSearch()">搜索</button>
            <span>&nbsp;<button @click="leaveEditAssign()">取消</button></span>
        </div>
        <ul>  
            <li v-for="station in showStationsInfo" :key="station.stationId">  
                <span>{{ station.stationName }}</span> 
                <span>&nbsp;&nbsp;{{ station.stationAddress }}</span>
                <span>&nbsp;<button @click="handleEditChoose(station.stationId)">选择</button></span>
            </li>  
        </ul> 
    </div>
</template>